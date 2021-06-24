using Godot;
using IsometricGame.Logic;

public class Lobby : CanvasLayer
{
	[Signal]
	public delegate void StartGameEvent();

	public override void _Ready()
	{
		base._Ready();
		this.GetNode<Button>("StartButton").Connect("pressed", this, nameof(OnStartButtonPressed));
	}

	public void OnStartButtonPressed()
	{
		EmitSignal(nameof(StartGameEvent));
	}
}
