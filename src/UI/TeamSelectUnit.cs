using Godot;
using IsometricGame.Logic;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using IsometricGame.Logic.ScriptHelpers;
using System;
using System.Linq;

public class TeamSelectUnit : VBoxContainer
{
	private TransferConnectData.UnitData Unit;

	[Signal]
	public delegate void UnitRemoved();

	public override void _Ready()
	{
		base._Ready();
		this.GetNode<OptionButton>("HBoxContainer/UnitTypeCombo").Connect("item_selected", this, nameof(UnitTypeChanged));
		this.GetNode<Button>("HBoxContainer/RemoveUnitButton").Connect("pressed", this, nameof(RemoveUnitButtonPressed));
		var newSkillCombo = this.GetNode<OptionButton>("SkillsContainer/NewSkillButton");
		newSkillCombo.Connect("item_selected", this, nameof(AddSkillButtonPressed));

		var skills = Enum.GetValues(typeof(Skill)).Cast<Skill>().ToList();
		var texture = ResourceLoader.Load<Texture>("assets/Skills.png");
		
		for (var i = 0; i < skills.Count; i++)
		{
			var skill = skills[i];

			var atlasTexture = new AtlasTexture();
			atlasTexture.Atlas = texture;
			atlasTexture.Region = new Rect2(((int)skill) % 4 * texture.GetSize().x / 4, ((int)skill) / 4 * texture.GetSize().y / 7, texture.GetSize().x / 12, texture.GetSize().y / 7);

			newSkillCombo.AddIconItem(atlasTexture, string.Empty);
		}

		newSkillCombo.Select(0);
	}

	public void AddSkillButtonPressed(int unitSkill)
	{
		var newSkillCombo = this.GetNode<OptionButton>("SkillsContainer/NewSkillButton");

		var skillsContainer = this.GetNode<Container>("SkillsContainer");
		var texture = ResourceLoader.Load<Texture>("assets/Skills.png");

		newSkillCombo.Select(0);
		
		this.Unit.Skills.Add((Skill)unitSkill);

		skillsContainer.AddChild(new TextureRect
		{
			Texture = new AtlasTexture
			{
				Atlas = texture,
				Region = new Rect2(unitSkill % 4 * texture.GetSize().x / 4, unitSkill / 4 * texture.GetSize().y / 7, texture.GetSize().x / 12, texture.GetSize().y / 7)
			}
		});

		UpdateUnitDetails();
	}

	private void RemoveUnitButtonPressed()
	{
		EmitSignal(nameof(UnitRemoved));
	}

	public void InitUnit(TransferConnectData.UnitData unit)
	{
		this.Unit = unit;
		var unitTypeCombo = this.GetNode<OptionButton>("HBoxContainer/UnitTypeCombo");
		var unitTypes = Enum.GetValues(typeof(UnitType)).Cast<UnitType>().ToList();
		for (var i = 0; i < unitTypes.Count; i++)
		{
			var unitType = unitTypes[i];
			unitTypeCombo.AddItem(unitType.ToString());
			if (unitType == unit.UnitType)
			{
				unitTypeCombo.Select(i);
			}
		}

		var skillsContainer = this.GetNode<Container>("SkillsContainer");
		var texture = ResourceLoader.Load<Texture>("assets/Skills.png");
		for (var j = 0; j < unit.Skills.Count; j++)
		{
			var unitSkill = (int)unit.Skills[j];

			skillsContainer.AddChild(new TextureRect
			{
				Texture = new AtlasTexture
				{
					Atlas = texture,
					Region = new Rect2(unitSkill % 4 * texture.GetSize().x / 4, unitSkill / 4 * texture.GetSize().y / 7, texture.GetSize().x / 12, texture.GetSize().y / 7)
				}
			});
		}

		UpdateUnitDetails();
	}

	private void UnitTypeChanged(int index)
	{
		this.Unit.UnitType = (UnitType)index;
		UpdateUnitDetails();
	}

	private void UpdateUnitDetails()
	{
		var unit = UnitUtils.BuildUnit(this.Unit.UnitType);
		var player = new ServerPlayer();
		for (var i = 0; i < this.Unit.Skills.Count; i++)
		{
			var skill = this.Unit.Skills[i];
			UnitUtils.ApplySkill(player, unit, skill);
		}

		this.GetNode<UnitDetails>("UnitDetails").SelectUnit(unit);

		this.Visible = false;
		this.CallDeferred("set_visible", true);
	}
}
