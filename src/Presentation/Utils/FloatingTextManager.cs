using Godot;
using System;

public class FloatingTextManager : Node2D
{
    [Export]
    public PackedScene FloatingTextScene;

    [Export]
    public Vector2 Direction = new Vector2(0, -80);

    [Export]
    public float Duration = 2;

    [Export]
    public float Spread = (float)Math.PI / 2;

    public void ShowValue(string value, Color? color = null, bool highlite = false)
    {
        var fct = (FloatingText)FloatingTextScene.Instance();
        AddChild(fct);
        fct.ShowValue(value, Direction, Duration, Spread, color, highlite);
    }
}
