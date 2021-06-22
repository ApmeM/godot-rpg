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
				button.RectPosition = Vector2.Right * (i + 1) * 100;
				button.Text = u.ToString();
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
