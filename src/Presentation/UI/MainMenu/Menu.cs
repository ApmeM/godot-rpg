using Godot;
using IsometricGame.Logic.Utils;
using IsometricGame.Presentation;
using IsometricGame.Repository;
using System.Threading.Tasks;

[SceneReference("Menu.tscn")]
public partial class Menu : Container
{
    private TeamsRepository teamsRepository;
    private AccountRepository accountRepository;
    private Communicator communicator;

    public int SelectedTeam => this.teamSelector.Selected;

    [Signal]
    public delegate void LoginDone(bool loginSuccess);

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
        this.communicator = GetNode<Communicator>("/root/Communicator");

        this.teamsRepository = DependencyInjector.teamsRepository;
        this.accountRepository = DependencyInjector.accountRepository;

        this.createButton.Connect("pressed", this, nameof(OnCreateButtonPressed));
        this.joinButton.Connect("pressed", this, nameof(OnJoinButtonPressed));

        this.campaignTextureButton.Connect("pressed", this, nameof(OnCampaignPressed));
        this.ladderTextureButton.Connect("pressed", this, nameof(OnLadderPressed));
        this.customTextureButton.Connect("pressed", this, nameof(OnCustomPressed));
        this.settingsTextureButton.Connect("pressed", this, nameof(OnSettingsPressed));
        this.exitTextureButton.Connect("pressed", this, nameof(OnExitPressed));

        this.loginLineEdit.Text = this.accountRepository.LoadLogin();
        this.passwordLineEdit.Text = this.accountRepository.LoadPassword();
        this.serverLineEdit.Text = this.accountRepository.LoadServer();

        this.teamSelector.Refresh(this.teamsRepository.LoadTeams());
    }

    #region Dashboard

    private async void OnLadderPressed()
    {
        if (string.IsNullOrWhiteSpace(this.loginLineEdit.Text))
        {
            this.loginDialog.PopupCentered();
            await ToSignal(this.loginDialog, "popup_hide");
        }

        await this.CreateConnection();
        this.ladderDialog.PopupCentered();
    }

    private async void OnCampaignPressed()
    {
        await this.CreateConnection(true);
        this.campaignDialog.PopupCentered();
    }

    private async void OnCustomPressed()
    {
        if (string.IsNullOrWhiteSpace(this.loginLineEdit.Text))
        {
            this.loginDialog.PopupCentered();
            await ToSignal(this.loginDialog, "popup_hide");
        }
        await this.CreateConnection();
        this.customDialog.PopupCentered();
    }

    private async void OnSettingsPressed()
    {
        this.loginDialog.PopupCentered();
        await ToSignal(this.loginDialog, "popup_hide");
        await this.CreateConnection();
    }

    private void OnExitPressed()
    {
        this.GetTree().Quit();
    }

    #endregion

    public void GameOver()
    {
    }

    #region Connection
    private async Task CreateConnection(bool createAsServer = false)
    {
        if (createAsServer || this.loginServerCheckbox.Pressed)
        {
            this.communicator.CreateServer();
        }
        else
        {
            this.communicator.CreateClient(this.serverLineEdit.Text, this.loginLineEdit.Text, this.passwordLineEdit.Text);
        }

        var isSuccess = false;

        while (!isSuccess)
        {
            isSuccess = (bool)(await ToSignal(this, nameof(LoginDone)))[0];
            if (!isSuccess)
            {
                loginIncorrectLabel.Visible = true;
                this.loginDialog.PopupCentered();
                await ToSignal(this.loginDialog, "popup_hide");
            }
        }

        this.accountRepository.SaveLogin(this.loginLineEdit.Text);
        this.accountRepository.SavePassword(this.passwordLineEdit.Text);
        this.accountRepository.SaveServer(this.serverLineEdit.Text);

        loginIncorrectLabel.Visible = true;
    }

    private void OnCreateButtonPressed()
    {
        this.communicator.CreateLobby();
    }

    #endregion

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
