using Godot;
using IsometricGame.Logic.Enums;
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

    public Vector2? NewPosition => shadow.NewPosition;
    public Ability? Ability => shadow.Ability;
    public Vector2? AbilityDirection => shadow.AbilityDirection;
    public Unit AbilityUnitTarget => shadow.AbilityUnitTarget;
    public bool IsDead { get; private set; }

    [Export]
    public PackedScene UnitShadowScene;

    private UnitShadow shadow;
    private FloatingTextManager hpPopup;

    [Signal]
    public delegate void MoveDone();
    [Signal]
    public delegate void UnitAnimationDone();

    public override void _Ready()
    {
        base._Ready();

        this.shadow = (UnitShadow)UnitShadowScene.Instance();
        this.shadow.Position = Position;
        this.shadow.Visible = false;
        this.GetParent<Maze>().AddChild(this.shadow);

        this.GetNode<AnimatedSprite>("AnimatedSprite").Frames = ResourceLoader.Load<SpriteFrames>($"Units/{ClientUnit.UnitType}.tres");
        this.shadow.GetNode<AnimatedSprite>("Shadow").Frames = ResourceLoader.Load<SpriteFrames>($"Units/{ClientUnit.UnitType}.tres");
        this.GetNode<TextureProgress>("TextureProgress").MaxValue = ClientUnit.MaxHp;
        this.GetNode<TextureProgress>("TextureProgress").Value = ClientUnit.Hp;

        this.hpPopup = this.GetNode<FloatingTextManager>("HpPopup");
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
    
    private void UnitAnimationFinished()
    {
        var animation = GetNode<AnimatedSprite>("AnimatedSprite");
        animation.Disconnect("animation_finished", this, nameof(UnitAnimationFinished));
        animation.Stop();
        EmitSignal(nameof(UnitAnimationDone));
    }

    private void AnimateUnit(string animationPrefix, Vector2 animationDirection)
    {
        var animation = GetNode<AnimatedSprite>("AnimatedSprite");
        animation.Connect("animation_finished", this, nameof(UnitAnimationFinished));
        animation.Play($"{animationPrefix}{IsometricMove.Animate(animationDirection)}");
    }

    public SignalAwaiter UnitHit(Vector2? attackFrom, int newHp)
    {
        var oldHp = this.ClientUnit.Hp;
        
        this.GetNode<TextureProgress>("TextureProgress").Value = newHp;
        if (this.ClientUnit.MaxHp * 0.75 < newHp)
        {
            this.GetNode<TextureProgress>("TextureProgress").TextureProgress_ = ResourceLoader.Load<Texture>("assets/Hp/Green.png");
        }
        else if (this.ClientUnit.MaxHp * 0.33 < newHp)
        {
            this.GetNode<TextureProgress>("TextureProgress").TextureProgress_ = ResourceLoader.Load<Texture>("assets/Hp/Yellow.png");
        }
        else
        {
            this.GetNode<TextureProgress>("TextureProgress").TextureProgress_ = ResourceLoader.Load<Texture>("assets/Hp/Red.png");
        }
        this.ClientUnit.Hp = newHp;
        if (newHp <= 0 && !IsDead)
        {
            this.IsDead = true;
            shadow.HideShadow();
            this.AnimateUnit("dead", attackFrom ?? Vector2.Up);
            return ToSignal(this, nameof(UnitAnimationDone));
        }
        else if (newHp > 0 && IsDead)
        {
            this.IsDead = false;
            this.AnimateUnit("hit", attackFrom.Value);
            return ToSignal(this, nameof(UnitAnimationDone));
        }
        else if (attackFrom.HasValue)
        {
            this.AnimateUnit("hit", attackFrom.Value);
            return ToSignal(this, nameof(UnitAnimationDone));
        }
        else if (oldHp != newHp)
        {
            this.AnimateUnit("hit", Vector2.Up);
            return ToSignal(this, nameof(UnitAnimationDone));
        }
        
        return ToSignal(GetTree().CreateTimer(0), "timeout");
    }

    public SignalAwaiter Attack(Vector2? attackDirection)
    {
        if (!attackDirection.HasValue)
        {
            return ToSignal(GetTree().CreateTimer(0), "timeout");
        }

        shadow.HideShadow();
        this.AnimateUnit("attack", attackDirection.Value);
        return ToSignal(this, nameof(UnitAnimationDone));
    }

    public SignalAwaiter MoveUnitTo(Vector2 newTarget)
    {
        var maze = GetParent<Maze>();
        var playerPosition = maze.WorldToMap(Position);
        if (playerPosition == newTarget)
        {
            return ToSignal(GetTree().CreateTimer(0), "timeout");
        }
        shadow.HideShadow();
        IsometricMove.MoveBy(this.path, Position, newTarget, maze);
        return ToSignal(this, nameof(MoveDone));
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

    public void AbilityShadowTo(Ability ability, Vector2 cell, Unit target = null)
    {
        if (!shadow.Visible)
        {
            shadow.Position = Position;
            shadow.Visible = true;
        }

        shadow.AbilityShadowTo(ability, cell, target);
    }

    public async void ShowChanges(List<int> changes)
    {
        if (changes == null)
        {
            return;
        }

        var sceneTree = GetTree();
        foreach (var change in changes)
        {
            this.hpPopup.ShowValue(change.ToString(), change > 0 ? new Color(0, 1, 0) : new Color(1, 0, 0));
            await ToSignal(sceneTree.CreateTimer(0.5f), "timeout");
        }
    }
}
