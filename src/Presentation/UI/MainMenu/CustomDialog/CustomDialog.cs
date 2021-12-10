using Godot;
using IsometricGame.Presentation;

[SceneReference("CustomDialog.tscn")]
public partial class CustomDialog : WindowDialog
{
    private Communicator communicator;

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
        this.communicator = GetNode<Communicator>("/root/Communicator");

        this.createButton.Connect("pressed", this, nameof(OnCreateButtonPressed));
        this.joinButton.Connect("pressed", this, nameof(OnJoinButtonPressed));
    }

    private void OnCreateButtonPressed()
    {
        this.communicator.CreateLobby();
    }

    private void OnJoinButtonPressed()
    {
        this.communicator.JoinLobby(this.lobbyIdLineEdit.Text);
    }
}
