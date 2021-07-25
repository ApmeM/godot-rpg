using Godot;
using IsometricGame.Logic;
using System;
using System.Collections.Generic;

public class Menu : Container
{
	private LineEdit lobbyId;
	private Login login;
	private TeamSelector teamSelector;
	private TabContainer onlineTabs;

	[Signal]
	public delegate void CreateLobby(int selectedTeam);
	[Signal]
	public delegate void JoinLobby(int selectedTeam, string lobbyId);

	public override void _Ready()
	{
		base._Ready();
		this.GetNode<Button>("VBoxContainer/TabContainer/Online/TabContainer/LoginContentContainer/ServerContainer/ServerButton").Connect("pressed", this, nameof(OnServerButtonPressed));
		this.GetNode<Button>("VBoxContainer/TabContainer/Online/TabContainer/LoginContentContainer/ClientContainer/ClientButton").Connect("pressed", this, nameof(OnClientButtonPressed));
		
		this.GetNode<Button>("VBoxContainer/TabContainer/Online/TabContainer/LobbyContentContainer/ActionContainer/CustomContainer/GridContainer/CreateButton").Connect("pressed", this, nameof(OnCreateButtonPressed));
		this.GetNode<Button>("VBoxContainer/TabContainer/Online/TabContainer/LobbyContentContainer/ActionContainer/CustomContainer/GridContainer/JoinButton").Connect("pressed", this, nameof(OnJoinButtonPressed));

		this.lobbyId = this.GetNode<LineEdit>("VBoxContainer/TabContainer/Online/TabContainer/LobbyContentContainer/ActionContainer/CustomContainer/GridContainer/lobbyIdLineEdit");
		this.login = this.GetNode<Login>("VBoxContainer/TabContainer/Online/TabContainer/LoginContentContainer/ClientContainer/Login");
		this.teamSelector = this.GetNode<TeamSelector>("VBoxContainer/TabContainer/Online/TabContainer/LobbyContentContainer/TeamSelector");
		this.onlineTabs = this.GetNode<TabContainer>("VBoxContainer/TabContainer/Online/TabContainer");
	}

	public void GameOver()
	{
	}

	private void OnServerButtonPressed()
	{
		GetNode<Communicator>("/root/Communicator").CreateServer();
		this.onlineTabs.CurrentTab = 1;
		this.teamSelector.Refresh(TransferConnectData.Load());
	}

	private void OnClientButtonPressed()
	{
		GetNode<Communicator>("/root/Communicator").CreateClient(login.ServerText);
		this.onlineTabs.CurrentTab = 1;
		this.teamSelector.Refresh(TransferConnectData.Load());
	}

	private void OnCreateButtonPressed()
	{
		EmitSignal(nameof(CreateLobby), this.teamSelector.Selected);
	}

	private void OnJoinButtonPressed()
	{
		EmitSignal(nameof(JoinLobby), this.teamSelector.Selected, lobbyId.Text);
	}
}
