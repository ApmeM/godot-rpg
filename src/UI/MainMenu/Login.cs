using Godot;

public class Login : GridContainer
{
	private LineEdit serverLineEdit;
	private LineEdit loginLineEdit;
	private LineEdit passwordLineEdit;

	[Export]
	public bool UseCustomServer;

	public override void _Ready()
	{
		this.serverLineEdit = GetNode<LineEdit>("ServerLineEdit");
		this.loginLineEdit = GetNode<LineEdit>("LoginLineEdit");
		this.passwordLineEdit = GetNode<LineEdit>("PasswordLineEdit");
	}

    public override void _Process(float delta)
    {
        base._Process(delta);
		if (this.UseCustomServer)
		{
			this.serverLineEdit.Text = "ApmeM.org";
			this.serverLineEdit.Editable = false;
		}
		else
		{
			this.serverLineEdit.Text = "127.0.0.1";
			this.serverLineEdit.Editable = true;
		}
	}

	public string ServerText => this.serverLineEdit.Text;
	public string LoginText => this.loginLineEdit.Text;
	public string PasswordText => this.passwordLineEdit.Text;
}
