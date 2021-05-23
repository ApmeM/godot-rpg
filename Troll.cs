using Godot;
using IsometricGame.Logic.Models;
using IsometricGame.Logic.ScriptHelpers;
using System.Collections.Generic;

public class Troll : Node2D
{
	private const int MOTION_SPEED = 800;
	private readonly Queue<Vector2> path = new Queue<Vector2>();
	private Vector2 oldTarget;
	
	public Unit Unit;
	public bool IsSelected;
	public Vector2 NewTarget = Vector2.Zero;

	private UnitShadow shadow;

	public override void _Ready()
	{
		base._Ready();

		this.shadow = (UnitShadow)ResourceLoader.Load<PackedScene>("UnitShadow.tscn").Instance();
		this.shadow.Position = Position;
		this.shadow.Visible = false;
		this.GetParent<Maze>().AddChild(this.shadow);
	}

	public override void _Process(float delta)
	{
		base._Process(delta);

		shadow.IsSelected = IsSelected;
		GetNode<AnimatedSprite>("SelectionMarker").Visible = IsSelected;

		var animation = GetNode<AnimatedSprite>("AnimatedSprite");

		var motion = IsometricMove.GetMotion(path, Position, delta, MOTION_SPEED);
		Position += motion ?? Vector2.Zero;

		animation.Playing = motion.HasValue;
		if (motion.HasValue)
		{
			animation.Animation = IsometricMove.Animate(motion.Value) ?? animation.Animation;
		}
	}

	public void MoveUnitTo(Vector2 newTarget)
	{
		if (newTarget != this.oldTarget)
		{
			this.oldTarget = newTarget;
			this.NewTarget = Vector2.Zero;
			shadow.Visible = false;

			IsometricMove.MoveBy(this.path, Position, newTarget, GetParent<Maze>());
		}
	}

	public void MoveShadowTo(Vector2 newTarget)
	{
		if (!shadow.Visible)
		{
			shadow.Position = Position;
			shadow.Visible = true;
		}
		this.NewTarget = newTarget;

		shadow.MoveShadowTo(newTarget);
	}
}
