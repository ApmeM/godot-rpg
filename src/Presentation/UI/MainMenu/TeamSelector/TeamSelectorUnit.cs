using Godot;
using IsometricGame.Logic;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.ScriptHelpers;
using IsometricGame.Logic.Utils;
using IsometricGame.Presentation;
using IsometricGame.Presentation.Utils;
using System;
using System.Linq;

[Tool]
[SceneReference("TeamSelectorUnit.tscn")]
public partial class TeamSelectorUnit : Control
{
    [Signal]
    public delegate void TeamSelected(TeamSelectorUnit teamSelectorUnit);


    private TransferConnectData team;
    private string caption;
    private bool captionDirty;
    private UnitType[] units;
    private bool unitsDirty;
    private bool isSelected;
    private bool isSelectedDirty;
    private readonly PluginUtils pluginUtils;

    public TransferConnectData Team
    {
        get => this.team;
        set
        {
            this.team = value;
            this.Refresh();
        }
    }

    [Export]
    public bool IsSelected
    {
        get => isSelected;
        set
        {
            this.isSelected = value;
            this.isSelectedDirty = true;
        }
    }

    [Export]
    public string Caption
    {
        get => this.caption;
        set
        {
            this.caption = value;
            this.captionDirty = true;
        }
    }

    [Export]
    public UnitType[] Units
    {
        get => this.units;
        set
        {
            this.units = value;
            this.unitsDirty = true;
        }
    }

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

    public void Refresh()
    {
        this.teamMembers.ClearChildren();
        this.Caption = this.team.TeamName;
        this.Units = this.team.Units.Select(a => a.UnitType).ToArray();
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        if (this.captionDirty)
        {
            this.captionDirty = false;
            this.teamNameLabel.Text = this.caption;
        }

        if (this.unitsDirty)
        {
            this.unitsDirty = false;
            this.teamMembers.ClearChildren();
            foreach (var unit in this.units)
            {
                var unitInstance = this.pluginUtils.FindUnitType(unit);
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

        if (this.isSelectedDirty)
        {
            this.isSelectedDirty = false;
            if (isSelected)
            {
                this.teamNameLabel.Modulate = new Color(0, 0, 1);
                this.EmitSignal(nameof(TeamSelected), this);
            }
            else
            {
                this.teamNameLabel.Modulate = new Color(1, 1, 1);
            }
        }
    }

    public void GuiInput(InputEvent @event)
    {
        base._GuiInput(@event);
        if (@event is InputEventMouseButton buttonEvent)
            if (buttonEvent.ButtonIndex == (int)ButtonList.Left && buttonEvent.IsPressed())
            {
                IsSelected = true;
            }
    }
}
