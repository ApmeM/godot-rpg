using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.ScriptHelpers;
using IsometricGame.Presentation;
using System.Collections.Generic;

[SceneReference("UnitShadow.tscn")]
public partial class UnitShadow : Node2D
{
    private const int MOTION_SPEED = 800;
    private readonly Queue<Vector2> path = new Queue<Vector2>();
    public Vector2? AbilityDirection;
    public Ability? Ability;
    public Unit AbilityUnitTarget;
    public Vector2? NewPosition;

    public bool IsSelected
    {
        get { return this.selectionMarker.Visible; }
        set { this.selectionMarker.Visible = value; }
    }

    public AnimatedSprite ShadowSprite => this.shadow;

    public override void _Process(float delta)
    {
        base._Process(delta);

        if (path.Count > 0)
        {
            var maze = GetParent<Maze>();
            var motion = maze.GetMotion(path, Position, delta, MOTION_SPEED);
            this.shadow.Playing = motion.HasValue;
            if (motion.HasValue)
            {
                Position += motion ?? Vector2.Zero;
                var direction = UnitUtils.Animate(motion.Value);
                if (!string.IsNullOrWhiteSpace(direction))
                {
                    this.shadow.Animation = $"move{direction}";
                }
            }
        }
    }

    public void MoveShadowTo(Vector2 newTarget)
    {
        var maze = GetParent<Maze>();
        maze.MoveBy(this.path, Position, newTarget, true);
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

        this.shadow.Playing = true;
        var direction = UnitUtils.Animate(AbilityDirection.Value);

        if (!string.IsNullOrWhiteSpace(direction))
        {
            this.shadow.Animation = $"attack{direction}";
        }
    }
}
