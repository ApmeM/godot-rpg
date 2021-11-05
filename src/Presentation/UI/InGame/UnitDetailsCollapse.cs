using Godot;
using IsometricGame.Logic.Models;
using IsometricGame.Presentation;

[SceneReference("UnitDetailsCollapse.tscn")]
public partial class UnitDetailsCollapse : Panel
{
    [Export]
    public int MoveSpeed = 400;

    public bool Displayed;
    
    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.close.Connect("pressed", this, nameof(CloseButtonPressed));
        this.close.Text = "<";
    }

    public void CloseButtonPressed()
    {
        Displayed = !Displayed;
        if (Displayed)
        {
            this.close.Text = ">";
        }
        else
        {
            this.close.Text = "<";
        }
    }

    public void SelectUnit(ClientUnit unit)
    {
        this.unitDetails.SelectUnit(unit);
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        if (Displayed && RectPosition.x >= GetViewportRect().Size.x - this.RectSize.x)
        {
            RectPosition += Vector2.Left * MoveSpeed * delta;
        }

        if (!Displayed && RectPosition.x <= GetViewportRect().Size.x)
        {
            RectPosition += Vector2.Right * MoveSpeed * delta;
        }
    }
}
