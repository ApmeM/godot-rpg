using Godot;
using IsometricGame.Logic.ScriptHelpers;

public class Menu : Container
{
    private LineEdit lobbyId;
    private TeamSelector teamSelector;
    private TabContainer onlineTabs;
    private Label incorrectLoginLabel;
    private Label joinLobbyLabel;
    private LineEdit loginText;
    private LineEdit serverText;
    private LineEdit passwordText;
    private Button serverButton;
    private Button clientButton;
    private Label serverLabel;
    private Communicator communicator;

    public int SelectedTeam => this.teamSelector.Selected;

    public override void _Ready()
    {
        base._Ready();
        this.serverButton = this.GetNode<Button>("VBoxContainer/TabContainer/Online/TabContainer/LoginContentContainer/ServerButton");
        this.clientButton = this.GetNode<Button>("VBoxContainer/TabContainer/Online/TabContainer/LoginContentContainer/ClientButton");
        this.serverLabel = this.GetNode<Label>("VBoxContainer/TabContainer/Online/TabContainer/LoginContentContainer/ServerLabel");

        this.GetNode<Button>("VBoxContainer/TabContainer/Online/TabContainer/LobbyContentContainer/ActionContainer/CustomContainer/GridContainer/CreateButton").Connect("pressed", this, nameof(OnCreateButtonPressed));
        this.GetNode<Button>("VBoxContainer/TabContainer/Online/TabContainer/LobbyContentContainer/ActionContainer/CustomContainer/GridContainer/JoinButton").Connect("pressed", this, nameof(OnJoinButtonPressed));

        this.incorrectLoginLabel = this.GetNode<Label>("VBoxContainer/TabContainer/Online/TabContainer/LoginContentContainer/IncorrectLoginLabel");
        this.joinLobbyLabel = this.GetNode<Label>("VBoxContainer/TabContainer/Online/TabContainer/LobbyContentContainer/ActionContainer/CustomContainer/GridContainer/JoinLabel");

        this.loginText = this.GetNode<LineEdit>("VBoxContainer/TabContainer/Online/TabContainer/LoginContentContainer/CredentialsContainer/LoginLineEdit");
        this.serverText = this.GetNode<LineEdit>("VBoxContainer/TabContainer/Online/TabContainer/LoginContentContainer/CredentialsContainer/ServerLineEdit");
        this.passwordText = this.GetNode<LineEdit>("VBoxContainer/TabContainer/Online/TabContainer/LoginContentContainer/CredentialsContainer/PasswordLineEdit");

        this.lobbyId = this.GetNode<LineEdit>("VBoxContainer/TabContainer/Online/TabContainer/LobbyContentContainer/ActionContainer/CustomContainer/GridContainer/lobbyIdLineEdit");
        this.teamSelector = this.GetNode<TeamSelector>("VBoxContainer/TabContainer/Online/TabContainer/LobbyContentContainer/TeamSelector");
        this.onlineTabs = this.GetNode<TabContainer>("VBoxContainer/TabContainer/Online/TabContainer");

        this.serverButton.Connect("pressed", this, nameof(OnServerButtonPressed));
        this.clientButton.Connect("pressed", this, nameof(OnClientButtonPressed));

        this.communicator = GetNode<Communicator>("/root/Communicator");

        if (OS.GetName() == "HTML5")
        {
            this.serverButton.Visible = false;
            this.serverLabel.Visible = false;
        }

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
        this.communicator.CreateServer();
    }

    private void OnClientButtonPressed()
    {
        this.communicator.CreateClient(this.serverText.Text, this.loginText.Text, this.passwordText.Text);
    }

    private void OnCreateButtonPressed()
    {
        this.communicator.CreateLobby();
    }

    public void LobbyCreated(string lobbyId)
    {
        this.communicator.JoinLobby(lobbyId);
    }

    private void OnJoinButtonPressed()
    {
        this.communicator.JoinLobby(this.lobbyId.Text);
    }

    public async void LobbyNotFound()
    {
        joinLobbyLabel.AddColorOverride("font_color", new Color(1, 0, 0));
        joinLobbyLabel.Text = "Lobby not found.";
        await ToSignal(GetTree().CreateTimer(3), "timeout");
        joinLobbyLabel.AddColorOverride("font_color", new Color(1, 1, 1));
        joinLobbyLabel.Text = "And join existing lobby";
    }
}
