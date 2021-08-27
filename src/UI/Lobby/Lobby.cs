using Godot;
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

        this.turnTimeoutCheckbox.Connect("pressed", this, nameof(OnTurnTimeoutPressed));
        this.startButton.Connect("pressed", this, nameof(OnStartButtonPressed));
        this.addBotButton.Connect("pressed", this, nameof(OnAddBotButtonPressed));
        this.GetNode<Button>("VBoxContainer/HBoxContainer/LeaveButton").Connect("pressed", this, nameof(OnLeaveButtonPressed));
    }

    private void OnTurnTimeoutPressed()
    {
        this.turnTimeoutSpinBox.Visible = this.turnTimeoutCheckbox.Pressed;
    }

    private void OnLeaveButtonPressed()
    {
        GetNode<Communicator>("/root/Communicator").LeaveLobby();
    }
    
    private void OnAddBotButtonPressed()
    {
        GetNode<Communicator>("/root/Communicator").AddBot();
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
        GetNode<Communicator>("/root/Communicator").StartGame( 
            fullMapCheckbox.Pressed, 
            turnTimeoutCheckbox.Pressed,
            (float)turnTimeoutSpinBox.Value);
    }
}
