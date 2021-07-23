using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using System.Collections.Generic;

public class UnitDetails : VBoxContainer
{
    private Label maxHpLabel;
    private Label moveRangeLabel;
    private Label sightRangeLabel;
    private Container skillsContainer;
    private Container effectsContainer;
    private Container abilityContainer;

    public override void _Ready()
	{
		base._Ready();

		maxHpLabel = GetNode<Label>("MaxHP");
		moveRangeLabel = GetNode<Label>("MoveRange");
		sightRangeLabel = GetNode<Label>("SightRange");
		skillsContainer = GetNode<Container>("SkillsContainer");
		effectsContainer = GetNode<Container>("EffectsContainer");
		abilityContainer = GetNode<Container>("AbilityContainer");
	}

	public void SelectUnit(ClientUnit unit)
	{
		maxHpLabel.Text = "MaxHP " + unit?.MaxHp.ToString() ?? "unknown";
		moveRangeLabel.Text = "Speed " + unit?.MoveDistance.ToString() ?? "unknown";
        sightRangeLabel.Text = "Vision " + unit?.SightRange.ToString() ?? "unknown";

		var texture = ResourceLoader.Load<Texture>("assets/Skills.png");
		foreach (Node node in skillsContainer.GetChildren())
		{
			node.QueueFree();
		}

		foreach (var skill in unit?.Skills ?? new HashSet<Skill>())
		{
			var unitSkill = (int)skill;
			var skillNode = new TextureRect
			{
				Texture = new AtlasTexture
				{
					Atlas = texture,
					Region = new Rect2(unitSkill % 4 * texture.GetSize().x / 4, unitSkill / 4 * texture.GetSize().y / 7, texture.GetSize().x / 12, texture.GetSize().y / 7)
				},
				Expand = true,
				StretchMode = TextureRect.StretchModeEnum.KeepAspect,
				RectMinSize = Vector2.One * 50
			};

			skillsContainer.AddChild(skillNode);
		}
		
		foreach (Node node in effectsContainer.GetChildren())
		{
			node.QueueFree();
		}

		foreach (var effect in unit?.Effects ?? new List<EffectDuration>())
		{
			var container = new CenterContainer();
			var effectNode = new TextureRect
			{
				Texture = ResourceLoader.Load<Texture>($"assets/Effects/{effect.Effect}.png"),
				Expand = true,
				StretchMode = TextureRect.StretchModeEnum.KeepAspect,
				RectMinSize = Vector2.One * 50
			};
			var durationNode = new Label
			{
				Text = effect.Duration.ToString()
			};
			container.AddChild(effectNode);
			container.AddChild(durationNode);

			effectsContainer.AddChild(container);
		}

		this.Visible = false;
		this.CallDeferred("set_visible", true);
	}
}
