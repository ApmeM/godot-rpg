using Godot;
using IsometricGame.Logic.Utils;
using IsometricGame.Presentation;
using IsometricGame.Repository;

[SceneReference("SettingsDialog.tscn")]
public partial class SettingsDialog : WindowDialog
{
    private readonly AccountRepository accountRepository;

    public string Login => this.loginLineEdit?.Text;
    public string Password => this.passwordLineEdit?.Text;
    public string Server => this.serverLineEdit?.Text;
    public bool IsServer => this.loginServerCheckbox?.Pressed ?? false;

    public SettingsDialog()
    {
        this.accountRepository = DependencyInjector.accountRepository;
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
        this.saveButton.Connect("pressed", this, nameof(SaveButtonPressed));
        this.resetButton.Connect("pressed", this, nameof(ResetButtonPressed));

        this.savedLabel.Visible = false;

        this.ResetButtonPressed();
    }

    private void ResetButtonPressed()
    {
        this.loginLineEdit.Text = this.accountRepository.LoadLogin();
        this.passwordLineEdit.Text = this.accountRepository.LoadPassword();
        this.serverLineEdit.Text = this.accountRepository.LoadServer();
        this.loginServerCheckbox.Pressed = this.accountRepository.LoadIsServer();
    }

    private async void SaveButtonPressed()
    {
        this.accountRepository.SaveLogin(this.loginLineEdit.Text);
        this.accountRepository.SavePassword(this.passwordLineEdit.Text);
        this.accountRepository.SaveServer(this.serverLineEdit.Text);
        this.accountRepository.SaveIsServer(this.loginServerCheckbox.Pressed);

        this.savedLabel.Visible = true;
        await ToSignal(this.GetTree().CreateTimer(5), "timeout");
        this.savedLabel.Visible = false;
    }
}
