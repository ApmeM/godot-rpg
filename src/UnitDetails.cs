using Godot;
using IsometricGame.Logic.Models;

public class UnitDetails : Panel
{
	[Export]
	public int MoveSpeed = 400;

	public bool Displayed;

	public override void _Ready()
	{
		this.GetNode<Button>("Close").Connect("pressed", this, nameof(CloseButtonPressed));
		RectPosition = new Vector2(GetViewportRect().Size.x, RectPosition.y);
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
		GetNode<Label>("MoveRange").Text = "Speed " + unit?.MoveDistance.ToString() ?? "unknown";
		GetNode<Label>("SightRange").Text = "Vision " + unit?.SightRange.ToString() ?? "unknown";
		GetNode<Label>("AttackDistance").Text = "Attack " + unit?.AttackDistance.ToString() ?? "unknown";
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
