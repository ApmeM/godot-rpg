using Godot;

public class Menu : Container
{
	[Signal]
	public delegate void CreateLobby();
	[Signal]
	public delegate void JoinLobby();

	public override void _Ready()
	{
		base._Ready();
		this.GetNode<Button>("VBoxContainer/TabContainer/Local network/ContentContainer/HBoxContainer/ServerContainer/ServerButton").Connect("pressed", this, nameof(OnCreateButtonPressed));
		this.GetNode<Button>("VBoxContainer/TabContainer/Local network/ContentContainer/HBoxContainer/ClientContainer/ClientButton").Connect("pressed", this, nameof(OnJoinButtonPressed));
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
}
