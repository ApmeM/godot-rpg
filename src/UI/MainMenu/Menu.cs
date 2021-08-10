using Godot;
using IsometricGame.Logic;
using IsometricGame.Logic.ScriptHelpers;

public class Menu : Container
{
    private LineEdit lobbyId;
    private TeamSelector teamSelector;
    private TabContainer onlineTabs;
    private Label incorrectLoginLabel;
    private LineEdit loginText;
    private LineEdit serverText;
    private LineEdit passwordText;

    [Signal]
    public delegate void CreateLobby(int selectedTeam);
    [Signal]
    public delegate void JoinLobby(int selectedTeam, string lobbyId);

    public override void _Ready()
    {
        base._Ready();
        this.GetNode<Button>("VBoxContainer/TabContainer/Online/TabContainer/LoginContentContainer/ServerButton").Connect("pressed", this, nameof(OnServerButtonPressed));
        this.GetNode<Button>("VBoxContainer/TabContainer/Online/TabContainer/LoginContentContainer/ClientButton").Connect("pressed", this, nameof(OnClientButtonPressed));

        this.GetNode<Button>("VBoxContainer/TabContainer/Online/TabContainer/LobbyContentContainer/ActionContainer/CustomContainer/GridContainer/CreateButton").Connect("pressed", this, nameof(OnCreateButtonPressed));
        this.GetNode<Button>("VBoxContainer/TabContainer/Online/TabContainer/LobbyContentContainer/ActionContainer/CustomContainer/GridContainer/JoinButton").Connect("pressed", this, nameof(OnJoinButtonPressed));

        this.incorrectLoginLabel = this.GetNode<Label>("VBoxContainer/TabContainer/Online/TabContainer/LoginContentContainer/IncorrectLoginLabel");

        this.loginText = this.GetNode<LineEdit>("VBoxContainer/TabContainer/Online/TabContainer/LoginContentContainer/CredentialsContainer/LoginLineEdit");
        this.serverText = this.GetNode<LineEdit>("VBoxContainer/TabContainer/Online/TabContainer/LoginContentContainer/CredentialsContainer/ServerLineEdit");
        this.passwordText = this.GetNode<LineEdit>("VBoxContainer/TabContainer/Online/TabContainer/LoginContentContainer/CredentialsContainer/PasswordLineEdit");

        this.lobbyId = this.GetNode<LineEdit>("VBoxContainer/TabContainer/Online/TabContainer/LobbyContentContainer/ActionContainer/CustomContainer/GridContainer/lobbyIdLineEdit");
        this.teamSelector = this.GetNode<TeamSelector>("VBoxContainer/TabContainer/Online/TabContainer/LobbyContentContainer/TeamSelector");
        this.onlineTabs = this.GetNode<TabContainer>("VBoxContainer/TabContainer/Online/TabContainer");

        this.teamSelector.Refresh(FileStorage.LoadTeams());

        var login = FileStorage.LoadLogin();
        if (string.IsNullOrWhiteSpace(login))
        {
            this.loginText.GrabFocus();
        }
        else
        {
            this.loginText.Text = login;
            this.passwordText.GrabFocus();
        }
    }

    public void GameOver()
    {
    }

    public void LoginSuccess()
    {
        this.onlineTabs.CurrentTab = 1;
        if (!string.IsNullOrWhiteSpace(this.loginText.Text))
        {
            FileStorage.SaveLogin(this.loginText.Text);
        }
    }

    public async void IncorrectLogin()
    {
        incorrectLoginLabel.Visible = true;
        await ToSignal(GetTree().CreateTimer(3), "timeout");
        incorrectLoginLabel.Visible = false;
    }

    private void OnServerButtonPressed()
    {
        GetNode<Communicator>("/root/Communicator").CreateServer();
    }

    private void OnClientButtonPressed()
    {
        GetNode<Communicator>("/root/Communicator").CreateClient(this.serverText.Text, this.loginText.Text, this.passwordText.Text);
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
