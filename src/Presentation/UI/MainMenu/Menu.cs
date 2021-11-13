using Godot;
using IsometricGame.Logic.Utils;
using IsometricGame.Presentation;
using IsometricGame.Repository;

[SceneReference("Menu.tscn")]
public partial class Menu : Container
{
    private TeamsRepository teamsRepository;
    private AccountRepository accountRepository;
    private Communicator communicator;

    public int SelectedTeam => this.teamSelector.Selected;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
        this.communicator = GetNode<Communicator>("/root/Communicator");

        this.teamsRepository = DependencyInjector.teamsRepository;
        this.accountRepository = DependencyInjector.accountRepository;

        this.createButton.Connect("pressed", this, nameof(OnCreateButtonPressed));
        this.joinButton.Connect("pressed", this, nameof(OnJoinButtonPressed));
        this.serverButton.Connect("pressed", this, nameof(OnServerButtonPressed));
        this.clientButton.Connect("pressed", this, nameof(OnClientButtonPressed));

        if (OS.GetName() == "HTML5")
        {
            this.serverButton.Visible = false;
            this.orLabel.Visible = false;
        }

        this.teamSelector.Refresh(this.teamsRepository.LoadTeams());

        var login = this.accountRepository.LoadLogin();
        if (string.IsNullOrWhiteSpace(login))
        {
            this.loginLineEdit.GrabFocus();
        }
        else
        {
            this.loginLineEdit.Text = login;
            this.passwordLineEdit.GrabFocus();
        }
    }

    public void GameOver()
    {
    }

    public void LoginSuccess()
    {
        this.onlineTabs.CurrentTab = 1;
        if (!string.IsNullOrWhiteSpace(this.loginLineEdit.Text))
        {
            this.accountRepository.SaveLogin(this.loginLineEdit.Text);
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
        this.communicator.CreateClient(this.serverLineEdit.Text, this.loginLineEdit.Text, this.passwordLineEdit.Text);
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
        this.communicator.JoinLobby(this.lobbyIdLineEdit.Text);
    }

    public async void LobbyNotFound()
    {
        joinLabel.AddColorOverride("font_color", new Color(1, 0, 0));
        joinLabel.Text = "Lobby not found.";
        await ToSignal(GetTree().CreateTimer(3), "timeout");
        joinLabel.AddColorOverride("font_color", new Color(1, 1, 1));
        joinLabel.Text = "And join existing lobby";
    }
}
