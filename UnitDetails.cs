using Godot;
using IsometricGame.Logic.Models;

public class UnitDetails : Panel
{
	public bool Displayed;

	public override void _Ready()
	{
		this.GetNode<Button>("Close").Connect("pressed", this, nameof(CloseButtonPressed));
	}

	public void CloseButtonPressed()
	{
		Displayed = false;
	}

	public void ShowUnit(ClientUnit unit)
	{
		Displayed = true;
		GetNode<Label>("MoveRange").Text = "Speed " + unit.MoveDistance;
		GetNode<Label>("SightRange").Text = "Vision " + unit.SightRange;
	}

	public override void _Process(float delta)
	{
		base._Process(delta);
		GD.Print(Displayed, RectPosition.x);

		if (Displayed && RectPosition.x >= 824)
		{
			RectPosition += Vector2.Left * 100 * delta;
		}

		if (!Displayed && RectPosition.x <= 1024)
		{
			RectPosition += Vector2.Right * 100 * delta;
		}
	}
}
