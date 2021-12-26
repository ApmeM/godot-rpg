using Godot;
using IsometricGame.Presentation;
using System.Threading.Tasks;

[SceneReference("Menu.tscn")]
public partial class Menu : Container
{
    private Communicator communicator;

    public int SelectedTeam => this.teamSelector.SelectedTeam;

    [Signal]
    public delegate void LoginDone(bool loginSuccess);
    
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
        this.communicator = GetNode<Communicator>("/root/Communicator");

        this.campaignTextureButton.Connect("pressed", this, nameof(OnCampaignPressed));
        this.ladderTextureButton.Connect("pressed", this, nameof(OnLadderPressed));
        this.customTextureButton.Connect("pressed", this, nameof(OnCustomPressed));
        this.settingsTextureButton.Connect("pressed", this, nameof(OnSettingsPressed));
        this.teamBuilderTextureButton.Connect("pressed", this, nameof(OnTeamBuilderPressed));
        this.exitTextureButton.Connect("pressed", this, nameof(OnExitPressed));
    }

    #region Dashboard

    private async void OnLadderPressed()
    {
        if (!await this.CreateConnection())
        {
            return;
        }
        this.teamSelectorPopup.Show();
        await ToSignal(this.teamSelector, nameof(TeamSelector.SelectionChanged));
        this.teamSelectorPopup.Hide();
        this.ladderDialog.PopupCentered();
    }

    private async void OnCampaignPressed()
    {
        if (!await this.CreateConnection(true))
        {
            return;
        }
        this.campaignDialog.PopupCentered();
    }

    private async void OnCustomPressed()
    {
        if (!await this.CreateConnection())
        {
            return;
        }
        this.teamSelectorPopup.Show();
        await ToSignal(this.teamSelector, nameof(TeamSelector.SelectionChanged));
        this.teamSelectorPopup.Hide();
        this.customDialog.PopupCentered();
    }

    private void OnSettingsPressed()
    {
        this.settingsDialog.PopupCentered();
    }

    private void OnTeamBuilderPressed()
    {
        this.teamBuilderDialog.PopupCentered();
    }

    private void OnExitPressed()
    {
        this.GetTree().Quit();
    }

    private async Task<bool> CreateConnection(bool createAsServer = false)
    {
        if (createAsServer || this.settingsDialog.IsServer)
        {
            this.communicator.CreateServer();
        }
        else
        {
            this.communicator.CreateClient(this.settingsDialog.Server, this.settingsDialog.Login, this.settingsDialog.Password);
        }

        var isSuccess = (bool)(await ToSignal(this, nameof(LoginDone)))[0];

        if (isSuccess)
        {
            return true;
        }

        var dialog = new AcceptDialog
        {
            WindowTitle = "Connection issue",
            DialogText = $"Failed to connect to {this.settingsDialog.Server}. \nPlease check: \n- your internet connection;\n- game settings.",
            PopupExclusive = true,
        };
        this.AddChild(dialog);
        dialog.PopupCenteredMinsize();
        await ToSignal(dialog, "popup_hide");
        dialog.QueueFree();
        return false;
    }

    #endregion

    public void GameOver()
    {
    }

    public async void LobbyNotFound(string lobbyId)
    {
        var dialog = new AcceptDialog
        {
            WindowTitle = "Not found",
            DialogText = $"Can't connect to lobby {lobbyId}. \nLobby not found",
            PopupExclusive = true,
        };
        this.AddChild(dialog);
        dialog.PopupCenteredMinsize();
        await ToSignal(dialog, "popup_hide");
        dialog.QueueFree();
    }
}
