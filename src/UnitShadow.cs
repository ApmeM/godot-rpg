using Godot;
using IsometricGame.Logic.ScriptHelpers;
using System.Collections.Generic;

public class UnitShadow : Node2D
{
	private const int MOTION_SPEED = 800;
	private readonly Queue<Vector2> path = new Queue<Vector2>();
	public Vector2? UsableDirection;
	public IUsable Usable;
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
		UsableDirection = null;
		Usable = null;
		NewPosition = newTarget;
	}

	public void HideShadow()
	{
		NewPosition = null;
		UsableDirection = null;
		Usable = null;
		Visible = false;
	}

	public void UsableShadowTo(Vector2 newTarget, IUsable usable)
	{
		var maze = GetParent<Maze>();
		this.UsableDirection = newTarget - maze.WorldToMap(Position);
		this.Usable = usable;

		var animation = GetNode<AnimatedSprite>("Shadow");
		animation.Playing = true;
		var direction = IsometricMove.Animate(UsableDirection.Value);

		if (!string.IsNullOrWhiteSpace(direction))
		{
			animation.Animation = $"attack{direction}";
		}
	}
}
