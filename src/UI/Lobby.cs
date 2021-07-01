using Godot;

public class Lobby : Container
{
	[Signal]
	public delegate void StartGameEvent();

	public override void _Ready()
	{
		base._Ready();
		this.GetNode<Button>("VBoxContainer/HBoxContainer/StartButton").Connect("pressed", this, nameof(OnStartButtonPressed));
	}

	public void OnStartButtonPressed()
	{
		EmitSignal(nameof(StartGameEvent));
	}
}
