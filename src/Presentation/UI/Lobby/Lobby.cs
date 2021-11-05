using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Presentation;
using System;

[SceneReference("Lobby.tscn")]
public partial class Lobby : Container
{
    private Communicator communicator;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        foreach (MapGeneratingType mapType in Enum.GetValues(typeof(MapGeneratingType)))
        {
            this.mapOptionButton.AddItem(mapType.ToString(), (int)mapType);
        }

        this.communicator = GetNode<Communicator>("/root/Communicator");

        this.turnTimeoutCheckbox.Connect("toggled", this, nameof(OnTurnTimeoutToggled));
        this.turnTimeoutCheckbox.Connect("toggled", this, nameof(OnSettingsChangedTurnTimeoutToggled));
        this.viewFullMapCheckbox.Connect("toggled", this, nameof(OnSettingsChangedFullMapToggled));
        this.turnTimeoutLineEdit.Connect("value_changed", this, nameof(OnSettingsChangedTurnTimeoutValueChanged));
        this.mapOptionButton.Connect("item_selected", this, nameof(OnSettingsChangedMapItemSelected));
        this.startButton.Connect("pressed", this, nameof(OnStartButtonPressed));
        this.addBotButton.Connect("pressed", this, nameof(OnAddBotButtonPressed));
        this.leaveButton.Connect("pressed", this, nameof(OnLeaveButtonPressed));
    }

    private void OnTurnTimeoutToggled(bool state)
    {
        this.turnTimeoutLineEdit.Visible = state;
    }

    private void OnSettingsChangedTurnTimeoutToggled(bool state)
    {
        this.OnSettingsChange();
    }

    private void OnSettingsChangedFullMapToggled(bool state)
    {
        this.OnSettingsChange();
    }

    private void OnSettingsChangedTurnTimeoutValueChanged(float state)
    {
        this.OnSettingsChange();
    }

    private void OnSettingsChangedMapItemSelected(int state)
    {
        this.OnSettingsChange();
    }

    private void OnLeaveButtonPressed()
    {
        this.communicator.LeaveLobby();
    }

    private void OnAddBotButtonPressed()
    {
        this.communicator.AddBot(Bot.Easy);
    }

    public void PlayerJoinedToLobby(string playerName)
    {
        this.playersListContainer.AddChild(new Label { Text = playerName });
    }

    public void YouJoinedToLobby(bool creator, string lobbyId, string playerName)
    {
        this.startButton.Visible = creator;
        this.addBotButton.Visible = creator;
        this.captionLabel.Text = lobbyId;

        this.viewFullMapCheckbox.Disabled = !creator;
        this.turnTimeoutCheckbox.Disabled = !creator;
        this.turnTimeoutLineEdit.Editable = creator;
        this.mapOptionButton.Disabled = !creator;

        foreach (Label child in this.playersListContainer.GetChildren())
        {
            child.QueueFree();
        }

        this.playersListContainer.AddChild(new Label { Text = playerName });
    }

    public void PlayerLeftLobby(string playerName)
    {
        foreach (Label child in this.playersListContainer.GetChildren())
        {
            if (child.Text == playerName)
            {
                child.QueueFree();
            }
        }
    }

    private void OnStartButtonPressed()
    {
        this.communicator.StartGame();
    }

    private void OnSettingsChange()
    {
        this.communicator.SyncConfig(
            viewFullMapCheckbox.Pressed,
            turnTimeoutCheckbox.Pressed,
            (float)turnTimeoutLineEdit.Value,
            (MapGeneratingType)this.mapOptionButton.Selected);
    }

    public void ConfigSynced(bool fullMapVisible, bool turnTimeoutEnaled, float turnTimeoutValue, MapGeneratingType mapType)
    {
        this.turnTimeoutCheckbox.Disconnect("toggled", this, nameof(OnSettingsChangedTurnTimeoutToggled));
        this.viewFullMapCheckbox.Disconnect("toggled", this, nameof(OnSettingsChangedFullMapToggled));
        this.turnTimeoutLineEdit.Disconnect("value_changed", this, nameof(OnSettingsChangedTurnTimeoutValueChanged));
        this.mapOptionButton.Disconnect("item_selected", this, nameof(OnSettingsChangedMapItemSelected));

        this.viewFullMapCheckbox.Pressed = fullMapVisible;
        this.turnTimeoutCheckbox.Pressed = turnTimeoutEnaled;
        this.turnTimeoutLineEdit.Value = turnTimeoutValue;
        this.mapOptionButton.Selected = (int)mapType;

        this.turnTimeoutCheckbox.Connect("toggled", this, nameof(OnSettingsChangedTurnTimeoutToggled));
        this.viewFullMapCheckbox.Connect("toggled", this, nameof(OnSettingsChangedFullMapToggled));
        this.turnTimeoutLineEdit.Connect("value_changed", this, nameof(OnSettingsChangedTurnTimeoutValueChanged));
        this.mapOptionButton.Connect("item_selected", this, nameof(OnSettingsChangedMapItemSelected));
    }
}
