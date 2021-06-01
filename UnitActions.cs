using Godot;
using IsometricGame.Logic.Enums;

public class UnitActions : Control
{
	[Signal]
	public delegate void ActionSelected(CurrentAction action);

	public override void _Ready()
	{
		this.GetNode<Button>("MoveButton").Connect("pressed", this, nameof(MoveButtonPressed));   
		this.GetNode<Button>("AttackButton").Connect("pressed", this, nameof(AttackButtonPressed));
	}

	private void MoveButtonPressed()
	{
		EmitSignal(nameof(ActionSelected), CurrentAction.Move);
	}

	private void AttackButtonPressed()
	{
		EmitSignal(nameof(ActionSelected), CurrentAction.Attack);
	}
}
