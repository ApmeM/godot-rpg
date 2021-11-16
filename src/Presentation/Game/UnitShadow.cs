using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.ScriptHelpers;
using IsometricGame.Logic.Utils;
using IsometricGame.Presentation;
using System.Collections.Generic;

[SceneReference("UnitShadow.tscn")]
public partial class UnitShadow : Node2D
{
    private const int MOTION_SPEED = 80;
    private readonly Queue<Vector2> path = new Queue<Vector2>();
    public Vector2? AbilityDirection;
    public Ability? Ability;
    public Unit AbilityUnitTarget;
    public Vector2? NewPosition;
    public Ability? NewPositionAbility;
    private readonly PluginUtils pluginUtils;

    public bool IsSelected
    {
        get { return this.selectionMarker.Visible; }
        set { this.selectionMarker.Visible = value; }
    }

    public AnimatedSprite ShadowSprite => this.shadow;


    public UnitShadow()
    {
        this.pluginUtils = DependencyInjector.pluginUtils;
    }

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

    public void MoveShadowTo(Vector2 newTarget, IMoveAbility moveAbility)
    {
        var maze = GetParent<Maze>();

        moveAbility.MoveBy(maze, Position, newTarget, this.path);
        Ability = null;
        AbilityDirection = null;
        AbilityUnitTarget = null;
        NewPosition = newTarget;
        NewPositionAbility = moveAbility.Ability;
    }

    public void HideShadow()
    {
        NewPosition = null;
        NewPositionAbility = null;
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
