using Godot;

public class Menu : Container
{
	[Signal]
	public delegate void CreateLobby();
	[Signal]
	public delegate void JoinLobby();
	[Signal]
	public delegate void ChangeTeam();

	public override void _Ready()
	{
		base._Ready();
		this.GetNode<Button>("VBoxContainer/GridContainer/ChangeTeamsButton").Connect("pressed", this, nameof(OnChangeTeamsButtonPressed));
		this.GetNode<Button>("VBoxContainer/GridContainer/CreateButton").Connect("pressed", this, nameof(OnCreateButtonPressed));
		this.GetNode<Button>("VBoxContainer/GridContainer/JoinButton").Connect("pressed", this, nameof(OnJoinButtonPressed));
	}

	public void GameOver()
	{
	}

	public void OnCreateButtonPressed()
	{
		EmitSignal(nameof(CreateLobby));
	}

	public void OnJoinButtonPressed()
	{
		EmitSignal(nameof(JoinLobby));
	}

	public void OnChangeTeamsButtonPressed()
	{
		EmitSignal(nameof(ChangeTeam));
	}
}
