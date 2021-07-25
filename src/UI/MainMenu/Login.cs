using Godot;

public class Login : GridContainer
{
	private LineEdit serverLineEdit;
	private LineEdit loginLineEdit;
	private LineEdit passwordLineEdit;

	public override void _Ready()
	{
		this.serverLineEdit = GetNode<LineEdit>("ServerLineEdit");
		this.loginLineEdit = GetNode<LineEdit>("LoginLineEdit");
		this.passwordLineEdit = GetNode<LineEdit>("PasswordLineEdit");
	}

	public string ServerText => this.serverLineEdit.Text;
	public string LoginText => this.loginLineEdit.Text;
	public string PasswordText => this.passwordLineEdit.Text;
}
