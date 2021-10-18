using Godot;
using IsometricGame.Logic.Enums;
using System;

public class Lobby : Container
{
    private Button startButton;
    private Button addBotButton;
    private VBoxContainer playersList;
    private Container settingsContainer;
    private Label captionLabel;
    private CheckBox fullMapCheckbox;
    private SpinBox turnTimeoutSpinBox;
    private CheckBox turnTimeoutCheckbox;
    private OptionButton mapOptionButton;
    private Communicator communicator;

    public override void _Ready()
    {
        base._Ready();
        this.startButton = this.GetNode<Button>("VBoxContainer/HBoxContainer/StartButton");
        this.addBotButton = this.GetNode<Button>("VBoxContainer/HBoxContainer/AddBotButton");
        this.playersList = this.GetNode<VBoxContainer>("VBoxContainer/HBoxContainer2/VBoxContainer");
        this.settingsContainer = this.GetNode<Container>("VBoxContainer/HBoxContainer2/VBoxContainer2");
        this.captionLabel = this.GetNode<Label>("VBoxContainer/CaptionLabel");
        this.fullMapCheckbox = settingsContainer.GetNode<CheckBox>("ViewFullMapCheckbox");
        this.turnTimeoutSpinBox = settingsContainer.GetNode<SpinBox>("TurnTimeoutLineEdit");
        this.turnTimeoutCheckbox = settingsContainer.GetNode<CheckBox>("TurnTimeoutCheckbox");
        this.mapOptionButton = GetNode<OptionButton>("VBoxContainer/HBoxContainer2/VBoxContainer2/MapOptionButton");
        foreach (MapGeneratingType mapType in Enum.GetValues(typeof(MapGeneratingType)))
        {
            this.mapOptionButton.AddItem(mapType.ToString(), (int)mapType);
        }

        this.communicator = GetNode<Communicator>("/root/Communicator");

        this.turnTimeoutCheckbox.Connect("toggled", this, nameof(OnTurnTimeoutToggled));
        this.turnTimeoutCheckbox.Connect("toggled", this, nameof(OnSettingsChangedTurnTimeoutToggled));
        this.fullMapCheckbox.Connect("toggled", this, nameof(OnSettingsChangedFullMapToggled));
        this.turnTimeoutSpinBox.Connect("value_changed", this, nameof(OnSettingsChangedTurnTimeoutValueChanged));
        this.mapOptionButton.Connect("item_selected", this, nameof(OnSettingsChangedMapItemSelected));
        this.startButton.Connect("pressed", this, nameof(OnStartButtonPressed));
        this.addBotButton.Connect("pressed", this, nameof(OnAddBotButtonPressed));
        this.GetNode<Button>("VBoxContainer/HBoxContainer/LeaveButton").Connect("pressed", this, nameof(OnLeaveButtonPressed));
    }

    private void OnTurnTimeoutToggled(bool state)
    {
        this.turnTimeoutSpinBox.Visible = state;
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
        this.playersList.AddChild(new Label { Text = playerName });
    }

    public void YouJoinedToLobby(bool creator, string lobbyId, string playerName)
    {
        this.startButton.Visible = creator;
        this.addBotButton.Visible = creator;
        this.captionLabel.Text = lobbyId;

        this.fullMapCheckbox.Disabled = !creator;
        this.turnTimeoutCheckbox.Disabled = !creator;
        this.turnTimeoutSpinBox.Editable = creator;
        this.mapOptionButton.Disabled = !creator;

        foreach (Label child in this.playersList.GetChildren())
        {
            child.QueueFree();
        }

        this.playersList.AddChild(new Label { Text = playerName });
    }

    public void PlayerLeftLobby(string playerName)
    {
        foreach (Label child in this.playersList.GetChildren())
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
            fullMapCheckbox.Pressed,
            turnTimeoutCheckbox.Pressed,
            (float)turnTimeoutSpinBox.Value,
            (MapGeneratingType)this.mapOptionButton.Selected);
    }

    public void ConfigSynced(bool fullMapVisible, bool turnTimeoutEnaled, float turnTimeoutValue, MapGeneratingType mapType)
    {
        this.turnTimeoutCheckbox.Disconnect("toggled", this, nameof(OnSettingsChangedTurnTimeoutToggled));
        this.fullMapCheckbox.Disconnect("toggled", this, nameof(OnSettingsChangedFullMapToggled));
        this.turnTimeoutSpinBox.Disconnect("value_changed", this, nameof(OnSettingsChangedTurnTimeoutValueChanged));
        this.mapOptionButton.Disconnect("item_selected", this, nameof(OnSettingsChangedMapItemSelected));

        this.fullMapCheckbox.Pressed = fullMapVisible;
        this.turnTimeoutCheckbox.Pressed = turnTimeoutEnaled;
        this.turnTimeoutSpinBox.Value = turnTimeoutValue;
        this.mapOptionButton.Selected = (int)mapType;

        this.turnTimeoutCheckbox.Connect("toggled", this, nameof(OnSettingsChangedTurnTimeoutToggled));
        this.fullMapCheckbox.Connect("toggled", this, nameof(OnSettingsChangedFullMapToggled));
        this.turnTimeoutSpinBox.Connect("value_changed", this, nameof(OnSettingsChangedTurnTimeoutValueChanged));
        this.mapOptionButton.Connect("item_selected", this, nameof(OnSettingsChangedMapItemSelected));
    }
}
