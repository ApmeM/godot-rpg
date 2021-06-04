using Godot;
using IsometricGame.Logic.Models;
using IsometricGame.Logic.ScriptHelpers;
using System.Collections.Generic;

public class Unit : Node2D
{
	private const int MOTION_SPEED = 800;
	private readonly Queue<Vector2> path = new Queue<Vector2>();
	public ClientUnit ClientUnit;
	public bool IsSelected
	{
		get { return GetNode<AnimatedSprite>("SelectionMarker").Visible; }
		set { GetNode<AnimatedSprite>("SelectionMarker").Visible = value; shadow.IsSelected = value; }
	}

	public Vector2 NewTarget => shadow.NewPosition ?? Vector2.Zero;
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

		var animation = GetNode<AnimatedSprite>("AnimatedSprite");
		if (path.Count > 0)
		{
			var motion = IsometricMove.GetMotion(path, Position, delta, MOTION_SPEED);
			animation.Playing = motion.HasValue;
			if (motion.HasValue)
			{
				Position += motion ?? Vector2.Zero;
				var direction = IsometricMove.Animate(motion.Value);
				if (!string.IsNullOrWhiteSpace(direction))
				{
					animation.Animation = $"move{direction}";
				}
			}
		}
	}

	public void MoveUnitTo(Vector2 newTarget)
	{
		shadow.HideShadow();
		IsometricMove.MoveBy(this.path, Position, newTarget, GetParent<Maze>());
	}

	public void MoveShadowTo(Vector2 newTarget)
	{
		if (!shadow.Visible)
		{
			shadow.Position = Position;
			shadow.Visible = true;
		}

		shadow.MoveShadowTo(newTarget);
	}

	public void AttackShadowTo(Vector2 newTarget)
	{
		if (!shadow.Visible)
		{
			shadow.Position = Position;
			shadow.Visible = true;
		}

		shadow.AttackShadowTo(newTarget);
	}
}
