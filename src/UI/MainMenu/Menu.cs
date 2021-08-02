using Godot;
using IsometricGame.Logic;
using System;
using System.Collections.Generic;

public class Menu : Container
{
    private LineEdit lobbyId;
    private TeamSelector teamSelector;
    private TabContainer onlineTabs;
    private LineEdit loginText;
    private LineEdit serverText;
    private LineEdit passwordText;

    [Signal]
    public delegate void CreateLobby(int selectedTeam, string playerName);
    [Signal]
    public delegate void JoinLobby(int selectedTeam, string lobbyId, string playerName);

    public override void _Ready()
    {
        base._Ready();
        this.GetNode<Button>("VBoxContainer/TabContainer/Online/TabContainer/LoginContentContainer/ConnectContentContainer/ServerContainer/ServerButton").Connect("pressed", this, nameof(OnServerButtonPressed));
        this.GetNode<Button>("VBoxContainer/TabContainer/Online/TabContainer/LoginContentContainer/ConnectContentContainer/ClientContainer/ClientButton").Connect("pressed", this, nameof(OnClientButtonPressed));
        
        this.GetNode<Button>("VBoxContainer/TabContainer/Online/TabContainer/LobbyContentContainer/ActionContainer/CustomContainer/GridContainer/CreateButton").Connect("pressed", this, nameof(OnCreateButtonPressed));
        this.GetNode<Button>("VBoxContainer/TabContainer/Online/TabContainer/LobbyContentContainer/ActionContainer/CustomContainer/GridContainer/JoinButton").Connect("pressed", this, nameof(OnJoinButtonPressed));

        this.loginText = this.GetNode<LineEdit>("VBoxContainer/TabContainer/Online/TabContainer/LoginContentContainer/LoginContainer/LoginLineEdit");
        this.serverText = this.GetNode<LineEdit>("VBoxContainer/TabContainer/Online/TabContainer/LoginContentContainer/ConnectContentContainer/ClientContainer/CredentialsContainer/ServerLineEdit");
        this.passwordText = this.GetNode<LineEdit>("VBoxContainer/TabContainer/Online/TabContainer/LoginContentContainer/ConnectContentContainer/ClientContainer/CredentialsContainer/PasswordLineEdit");

        this.lobbyId = this.GetNode<LineEdit>("VBoxContainer/TabContainer/Online/TabContainer/LobbyContentContainer/ActionContainer/CustomContainer/GridContainer/lobbyIdLineEdit");
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
        GetNode<Communicator>("/root/Communicator").CreateClient(this.serverText.Text);
        this.onlineTabs.CurrentTab = 1;
        this.teamSelector.Refresh(TransferConnectData.Load());
    }

    private void OnCreateButtonPressed()
    {
        EmitSignal(nameof(CreateLobby), this.teamSelector.Selected, this.loginText.Text);
    }

    private void OnJoinButtonPressed()
    {
        EmitSignal(nameof(JoinLobby), this.teamSelector.Selected, lobbyId.Text, this.loginText.Text);
    }
}
