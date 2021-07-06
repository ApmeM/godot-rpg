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
			var label = new Label();
			label.Text = ability.ToString();

			abilityContainer.AddChild(label);
		}

		this.Visible = false;
		this.CallDeferred("set_visible", true);
	}
}
