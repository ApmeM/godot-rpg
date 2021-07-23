using Godot;
using IsometricGame.Logic.Models;

public class UnitDetailsCollapse : Panel
{
	[Export]
	public int MoveSpeed = 400;

	public bool Displayed;
    
	private UnitDetails unitDetails;
    private Button closeButton;

    public override void _Ready()
	{
		this.unitDetails = this.GetNode<UnitDetails>("UnitDetails");
		this.closeButton = this.GetNode<Button>("Close");

		this.closeButton.Connect("pressed", this, nameof(CloseButtonPressed));
		this.closeButton.Text = "<";
	}

	public void CloseButtonPressed()
	{
		Displayed = !Displayed;
		if (Displayed)
		{
			this.closeButton.Text = ">";
		}
		else
		{
			this.closeButton.Text = "<";
		}
	}

	public void SelectUnit(ClientUnit unit)
	{
		this.unitDetails.SelectUnit(unit);
	}

	public override void _Process(float delta)
	{
		base._Process(delta);

		if (Displayed && RectPosition.x >= GetViewportRect().Size.x - this.RectSize.x)
		{
			RectPosition += Vector2.Left * MoveSpeed * delta;
		}

		if (!Displayed && RectPosition.x <= GetViewportRect().Size.x)
		{
			RectPosition += Vector2.Right * MoveSpeed * delta;
		}
	}
}
