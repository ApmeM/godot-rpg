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

	public Vector2? NewTarget => shadow.NewPosition;
	public Vector2? AttackDirection => shadow.AttackDirection;

	private UnitShadow shadow;

	[Signal]
	public delegate void MoveDone();
	[Signal]
	public delegate void AttackDone();
	[Signal]
	public delegate void UnitHitDone();

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

			if (path.Count == 0)
			{
				this.EmitSignal(nameof(MoveDone));
			}
		}
	}

	public void AttackAnimationFinished()
	{
		var animation = GetNode<AnimatedSprite>("AnimatedSprite");
		animation.Disconnect("animation_finished", this, nameof(AttackAnimationFinished));
		EmitSignal(nameof(AttackDone));
		animation.Playing = false;
	}

	public void AttackUnitTo(Vector2 attackDirection)
	{
		var animation = GetNode<AnimatedSprite>("AnimatedSprite");
		var direction = IsometricMove.Animate(attackDirection);

		animation.Animation = $"attack{direction}";
		animation.Connect("animation_finished", this, nameof(AttackAnimationFinished));
		animation.Playing = true;
	}

	public void UnitHitAnimationFinished()
	{
		var animation = GetNode<AnimatedSprite>("AnimatedSprite");
		animation.Disconnect("animation_finished", this, nameof(UnitHitAnimationFinished));
		EmitSignal(nameof(UnitHitDone));
		animation.Playing = false;
	}

	public void UnitHit(Vector2 attackDirection)
	{
		var animation = GetNode<AnimatedSprite>("AnimatedSprite");
		var direction = IsometricMove.Animate(attackDirection);

		animation.Animation = $"hit{direction}";
		animation.Connect("animation_finished", this, nameof(UnitHitAnimationFinished));
		animation.Playing = true;
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
