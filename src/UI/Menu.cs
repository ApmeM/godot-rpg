using Godot;

public class Menu : CanvasLayer
{
	[Signal]
	public delegate void CreateLobby();
	[Signal]
	public delegate void ChangeTeam();

	public override void _Ready()
	{
		base._Ready();
		this.GetNode<Button>("ChangeTeamsButton").Connect("pressed", this, nameof(OnChangeTeamsButtonPressed));
		this.GetNode<Button>("CreateButton").Connect("pressed", this, nameof(OnStartButtonPressed));
		this.GetNode<Button>("JoinButton").Connect("pressed", this, nameof(OnStartButtonPressed));
	}

	public void GameOver()
	{
	}

	public void OnStartButtonPressed()
	{
		EmitSignal(nameof(CreateLobby));
	}
	public void OnChangeTeamsButtonPressed()
	{
		EmitSignal(nameof(ChangeTeam));
	}
}
