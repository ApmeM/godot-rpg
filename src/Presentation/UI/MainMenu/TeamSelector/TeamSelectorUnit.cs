using Godot;
using IsometricGame.Logic;
using IsometricGame.Logic.ScriptHelpers;
using IsometricGame.Logic.Utils;
using IsometricGame.Presentation;
using IsometricGame.Presentation.Utils;

[SceneReference("TeamSelectorUnit.tscn")]
public partial class TeamSelectorUnit : Control
{
    [Signal]
    public delegate void Selected(TeamSelectorUnit teamSelectorUnit);

    public int TeamId = -1;
    private readonly PluginUtils pluginUtils;

    public TeamSelectorUnit()
    {
        this.pluginUtils = DependencyInjector.pluginUtils;
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.teamMembers.ClearChildren();
        
        this.Connect("gui_input", this, nameof(GuiInput));
        this.backgroundPanel.Connect("gui_input", this, nameof(GuiInput));
        this.borderContainer.Connect("gui_input", this, nameof(GuiInput));
        this.contentContainer.Connect("gui_input", this, nameof(GuiInput));
    }

    public void GuiInput(InputEvent @event)
    {
        base._GuiInput(@event);
        if (@event is InputEventMouseButton buttonEvent)
            if (buttonEvent.ButtonIndex == (int)ButtonList.Left && buttonEvent.IsPressed())
            {
                Select();
            }
    }

    public void Initialize(TransferConnectData team, int teamId)
    {
        this.teamMembers.ClearChildren();

        this.TeamId = teamId;
        this.teamNameLabel.Text = team.TeamName;
        foreach (var unit in team.Units)
        {
            var unitInstance = this.pluginUtils.FindUnitType(unit.UnitType);
            var frame = unitInstance.GetFrames().GetFrame("moveRight", 0);

            var label = new TextureRect()
            {
                Texture = frame,
                Expand = true,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                RectMinSize = Vector2.One * 50,
            };

            this.teamMembers.AddChild(label);
        }
    }

    public void Select()
    {
        this.EmitSignal(nameof(Selected), this);
    }

    public void Deselect()
    {
    }
}
