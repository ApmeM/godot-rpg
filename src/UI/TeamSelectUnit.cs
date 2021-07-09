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
		this.GetNode<Button>("ManageSkillsButton").Connect("pressed", this, nameof(ButtonPressed));
		this.GetNode<OptionButton>("HBoxContainer/UnitTypeCombo").Connect("item_selected", this, nameof(UnitTypeChanged));
		this.GetNode<Button>("HBoxContainer/RemoveUnitButton").Connect("pressed", this, nameof(RemoveUnitButtonPressed));

		var skills = Enum.GetValues(typeof(Skill)).Cast<Skill>().ToList();
		var texture = ResourceLoader.Load<Texture>("assets/Skills.png");

		for (var i = 0; i < skills.Count; i++)
		{
			var unitSkill = (int)skills[i];

			var skillNode = new TextureButton
			{
				TextureNormal = new AtlasTexture
				{
					Atlas = texture,
					Region = new Rect2(unitSkill % 4 * texture.GetSize().x / 4, unitSkill / 4 * texture.GetSize().y / 7, texture.GetSize().x / 12, texture.GetSize().y / 7)
				},
				Expand = true,
				StretchMode = TextureButton.StretchModeEnum.KeepAspect,
				RectMinSize = Vector2.One * 50
			};

			skillNode.Connect("pressed", this, nameof(AddSkillButtonPressed), new Godot.Collections.Array { unitSkill });
			this.GetNode<Container>("WindowDialog/GridContainer").AddChild(skillNode);
		}
	}

	private void ButtonPressed()
	{
		this.GetNode<WindowDialog>("WindowDialog").PopupCentered();
	}

	public void AddSkillButtonPressed(int unitSkill)
	{
		this.Unit.Skills.Add((Skill)unitSkill);

		AddSkillButton(unitSkill);

		UpdateUnitDetails();
	}

	private void AddSkillButton(int unitSkill)
	{
		var skillsContainer = this.GetNode<Container>("SkillsContainer");
		var texture = ResourceLoader.Load<Texture>("assets/Skills.png");
		var skillNode = new TextureButton
		{
			TextureNormal = new AtlasTexture
			{
				Atlas = texture,
				Region = new Rect2(unitSkill % 4 * texture.GetSize().x / 4, unitSkill / 4 * texture.GetSize().y / 7, texture.GetSize().x / 12, texture.GetSize().y / 7)
			},
			Expand = true,
			StretchMode = TextureButton.StretchModeEnum.KeepAspect,
			RectMinSize = Vector2.One * 50
		};
		skillNode.Connect("pressed", this, nameof(RemoveSkillButtonPressed), new Godot.Collections.Array { skillNode });
		skillsContainer.AddChild(skillNode);
	}

	private void RemoveSkillButtonPressed(Node node)
	{
		var index = node.GetParent<Container>().GetChildren().IndexOf(node);
		this.Unit.Skills.RemoveAt(index);
		node.QueueFree();
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

		for (var j = 0; j < unit.Skills.Count; j++)
		{
			AddSkillButton((int)unit.Skills[j]);
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
