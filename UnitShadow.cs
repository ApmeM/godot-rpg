using Godot;
using IsometricGame.Logic.ScriptHelpers;
using System.Collections.Generic;

public class UnitShadow : Node2D
{
	private const int MOTION_SPEED = 800;
	private readonly Queue<Vector2> path = new Queue<Vector2>();
	public bool IsSelected;

	public override void _Process(float delta)
	{
		base._Process(delta);

		var animation = GetNode<AnimatedSprite>("Shadow");
		GetNode<AnimatedSprite>("SelectionMarker").Visible = IsSelected;

		var motion = IsometricMove.GetMotion(path, Position, delta, MOTION_SPEED);

		Position += motion ?? Vector2.Zero;

		animation.Playing = motion.HasValue;
		if (motion.HasValue)
		{
			animation.Animation = IsometricMove.Animate(motion.Value) ?? animation.Animation;
		}
	}

	public void MoveShadowTo(Vector2 newTarget)
	{
		IsometricMove.MoveBy(this.path, Position, newTarget, GetParent<Maze>());
	}
}
