using Godot;
using IsometricGame.Logic.Models;

public class UnitDetails : VBoxContainer
{
	public void SelectUnit(ClientUnit unit)
	{
		GetNode<Label>("MaxHP").Text = "MaxHP " + unit?.MaxHp.ToString() ?? "unknown";
		GetNode<Label>("MoveRange").Text = "Speed " + unit?.MoveDistance.ToString() ?? "unknown";
		GetNode<Label>("SightRange").Text = "Vision " + unit?.SightRange.ToString() ?? "unknown";
		GetNode<Label>("AttackDistance").Text = "Attack " + unit?.AttackDistance.ToString() ?? "unknown";
	}

	public void SelectUnit(ServerUnit unit)
	{
		GetNode<Label>("MaxHP").Text = "MaxHP " + unit?.MaxHp.ToString() ?? "unknown";
		GetNode<Label>("MoveRange").Text = "Speed " + unit?.MoveDistance.ToString() ?? "unknown";
		GetNode<Label>("SightRange").Text = "Vision " + unit?.SightRange.ToString() ?? "unknown";
		GetNode<Label>("AttackDistance").Text = "Attack " + unit?.AttackDistance.ToString() ?? "unknown";
	}
}
