using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using System.Collections.Generic;

public class UnitDetails : VBoxContainer
{
	public void SelectUnit(ClientUnit unit)
	{
		GetNode<Label>("MaxHP").Text = "MaxHP " + unit?.MaxHp.ToString() ?? "unknown";
		GetNode<Label>("MoveRange").Text = "Speed " + unit?.MoveDistance.ToString() ?? "unknown";
		GetNode<Label>("SightRange").Text = "Vision " + unit?.SightRange.ToString() ?? "unknown";

		var texture = ResourceLoader.Load<Texture>("assets/Skills.png");
		var skillsContainer = GetNode<Container>("SkillsContainer");
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

		this.Visible = false;
		this.CallDeferred("set_visible", true);
	}

	public void SelectUnit(ServerUnit unit)
	{
		GetNode<Label>("MaxHP").Text = "MaxHP " + unit?.MaxHp.ToString() ?? "unknown";
		GetNode<Label>("MoveRange").Text = "Speed " + unit?.MoveDistance.ToString() ?? "unknown";
		GetNode<Label>("SightRange").Text = "Vision " + unit?.SightRange.ToString() ?? "unknown";
		var abilityContainer = GetNode<Container>("AbilityContainer");
		foreach (Node node in abilityContainer.GetChildren())
		{
			node.QueueFree();
		}

		foreach (var ability in unit?.Abilities ?? new HashSet<Ability>())
		{
			var abilityNode = new TextureRect
			{
				Texture = ResourceLoader.Load<Texture>($"assets/Abilities/{ability}.png"),
				Expand = true,
				StretchMode = TextureRect.StretchModeEnum.KeepAspect,
				RectMinSize = Vector2.One * 50
			};

			abilityContainer.AddChild(abilityNode);
		}

		this.Visible = false;
		this.CallDeferred("set_visible", true);
	}
}
