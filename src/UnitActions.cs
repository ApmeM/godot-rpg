using Godot;
using IsometricGame.Logic.Enums;
using System.Collections.Generic;

public class UnitActions : Control
{
	[Signal]
	public delegate void ActionSelected(CurrentAction action, Usable usableName);

	private readonly List<Button> buttons = new List<Button>();

	public List<Usable> Usables
	{
		set
		{
			foreach(var b in buttons)
			{
				b.Disconnect("pressed", this, nameof(AttackButtonPressed));
				this.RemoveChild(b);
			}
			buttons.Clear();

			for (var i = 0; i < value.Count; i++)
			{
				var u = value[i];
				var button = new Button();
				buttons.Add(button);
				this.AddChild(button);
				button.Connect("pressed", this, nameof(AttackButtonPressed), new Godot.Collections.Array { u });
				button.RectPosition = Vector2.Right * 60 + Vector2.Down * i * 70 - Vector2.Down * 33;
				button.Text = u.ToString();
				button.RectSize = new Vector2(button.RectSize.x, 66);
			}
		}
	}

	public override void _Ready()
	{
		this.GetNode<Button>("MoveButton").Connect("pressed", this, nameof(MoveButtonPressed));
	}

	private void MoveButtonPressed()
	{
		EmitSignal(nameof(ActionSelected), CurrentAction.Move, Usable.None);
	}

	private void AttackButtonPressed(Usable usable)
	{
		EmitSignal(nameof(ActionSelected), CurrentAction.UsableSkill, usable);
	}
}
