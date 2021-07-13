using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.ScriptHelpers;
using System.Collections.Generic;

public class UnitShadow : Node2D
{
	private const int MOTION_SPEED = 800;
	private readonly Queue<Vector2> path = new Queue<Vector2>();
	public Vector2? AbilityDirection;
	public Ability? Ability;
    public Unit AbilityUnitTarget;
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
		Ability = null;
		AbilityDirection = null;
		AbilityUnitTarget = null;
		NewPosition = newTarget;
	}

	public void HideShadow()
	{
		NewPosition = null;
		Ability = null;
		AbilityDirection = null;
		AbilityUnitTarget = null;
		Visible = false;
	}

	public void AbilityShadowTo(Ability ability, Vector2 abilityCellTarget, Unit abilityUnitTarget)
    {
		var maze = GetParent<Maze>();
		this.Ability = ability;
		this.AbilityDirection = abilityCellTarget - maze.WorldToMap(Position);
		this.AbilityUnitTarget = abilityUnitTarget;

		var animation = GetNode<AnimatedSprite>("Shadow");
		animation.Playing = true;
		var direction = IsometricMove.Animate(AbilityDirection.Value);

		if (!string.IsNullOrWhiteSpace(direction))
		{
			animation.Animation = $"attack{direction}";
		}
	}
}
