using Godot;
using IsometricGame.Logic.ScriptHelpers;
using System.Collections.Generic;

public class UnitShadow : Node2D
{
	private const int MOTION_SPEED = 800;
	private readonly Queue<Vector2> path = new Queue<Vector2>();
	public Vector2? AttackDirection;
	public Vector2? NewPosition;

	public bool IsSelected
	{
		get { return GetNode<AnimatedSprite>("SelectionMarker").Visible; }
		set { GetNode<AnimatedSprite>("SelectionMarker").Visible = value; }
	}

	public override void _Process(float delta)
	{
		base._Process(delta);

		var animation = GetNode<AnimatedSprite>("Shadow");

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

	public void MoveShadowTo(Vector2 newTarget)
	{
		IsometricMove.MoveBy(this.path, Position, newTarget, GetParent<Maze>());
		AttackDirection = null;
		NewPosition = newTarget;
	}

	public void HideShadow()
	{
		NewPosition = null;
		AttackDirection = null;
		Visible = false;
	}

	public void AttackShadowTo(Vector2 newTarget)
	{
		var maze = GetParent<Maze>();
		AttackDirection = newTarget - maze.WorldToMap(Position);

		var animation = GetNode<AnimatedSprite>("Shadow");
		animation.Playing = true;
		var direction = IsometricMove.Animate(AttackDirection.Value);

		if (!string.IsNullOrWhiteSpace(direction))
		{
			animation.Animation = $"attack{direction}";
		}
	}
}
