using Godot;
using IsometricGame.Logic.Models;

public class UnitDetailsCollapse : Panel
{
	[Export]
	public int MoveSpeed = 400;

	public bool Displayed;

	public override void _Ready()
	{
		this.GetNode<Button>("Close").Connect("pressed", this, nameof(CloseButtonPressed));
		this.GetNode<Button>("Close").Text = "<";
	}

	public void CloseButtonPressed()
	{
		Displayed = !Displayed;
		if (Displayed)
		{
			this.GetNode<Button>("Close").Text = ">";
		}
		else
		{
			this.GetNode<Button>("Close").Text = "<";
		}
	}

	public void SelectUnit(ClientUnit unit)
	{
		GetNode<UnitDetails>("UnitDetails").SelectUnit(unit);
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
