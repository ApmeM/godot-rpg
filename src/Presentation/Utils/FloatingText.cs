using Godot;

public class FloatingText : Label
{
    public async void ShowValue(string value, Vector2 direction, float duration, float spread, Color? color = null, bool highlite = false)
    {
        var tween = GetNode<Tween>("Tween");

        this.Modulate = color ?? new Color(1, 1, 1);

        this.Text = value;
        var movement = direction.Rotated((float)GD.RandRange(-spread / 2, spread / 2));
        this.RectPivotOffset = this.RectSize / 2;

        tween.InterpolateProperty(this, "rect_position", this.RectPosition, this.RectPosition + movement, duration, Tween.TransitionType.Linear, Tween.EaseType.InOut);
        tween.InterpolateProperty(this, "modulate:a", 1.0, 0.0, duration, Tween.TransitionType.Linear, Tween.EaseType.InOut);

        if (highlite)
        {
            tween.InterpolateProperty(this, "rect_scale", this.RectScale * 2, this.RectScale, 0.4f, Tween.TransitionType.Back, Tween.EaseType.In);
        }

        tween.Start();
        await ToSignal(tween, "tween_all_completed");
        QueueFree();
    }
}
