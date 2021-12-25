using Godot;
using IsometricGame.Logic;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using IsometricGame.Logic.ScriptHelpers;
using IsometricGame.Logic.Utils;
using IsometricGame.Presentation;
using IsometricGame.Presentation.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

[SceneReference("TeamSelectUnit.tscn")]
public partial class TeamSelectUnit : VBoxContainer
{
    private TransferConnectData.UnitData Unit;

    private readonly UnitUtils unitUtils;
    private readonly PluginUtils pluginUtils;

    public TeamSelectUnit()
    {
        this.unitUtils = DependencyInjector.unitUtils;
        this.pluginUtils = DependencyInjector.pluginUtils;
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();

        this.manageSkillsButton.Connect("pressed", this, nameof(ManageSkillButtonPressed));
        this.unitTypeCombo.Connect("item_selected", this, nameof(UnitTypeChanged));

        var skills = Enum.GetValues(typeof(Skill)).Cast<Skill>().ToList();
        var texture = ResourceLoader.Load<Texture>("assets/Skills.png");
        availableSkillsContainer.ClearChildren();
        skillsContainer.ClearChildren();
        abilityContainer.ClearChildren();
        for (var i = 0; i < skills.Count; i++)
        {
            var unitSkill = (int)skills[i];

            var skillNode = new TextureButton
            {
                TextureNormal = new AtlasTexture
                {
                    Atlas = texture,
                    Region = new Rect2(unitSkill % 4 * texture.GetSize().x / 4, unitSkill / 4 * texture.GetSize().y / 7, texture.GetSize().x / 12, texture.GetSize().y / 7)
                },
                Expand = true,
                StretchMode = TextureButton.StretchModeEnum.KeepAspect,
                RectMinSize = Vector2.One * 50,
                HintTooltip = this.pluginUtils.FindSkill((Skill)unitSkill).Description
            };

            skillNode.Connect("pressed", this, nameof(AddSkillButtonPressed), new Godot.Collections.Array { unitSkill });
            availableSkillsContainer.AddChild(skillNode);
        }
    }

    private void ManageSkillButtonPressed()
    {
        windowDialog.PopupCentered();
    }

    public void AddSkillButtonPressed(int unitSkill)
    {
        if(this.Unit.Skills.Count == ServerConfiguration.DefaultMaxSkills)
        {
            return;
        }

        this.Unit.Skills.Add((Skill)unitSkill);

        AddSkillButton(unitSkill);

        UpdateUnitDetails();
    }

    private void AddSkillButton(int unitSkill)
    {
        var texture = ResourceLoader.Load<Texture>("assets/Skills.png");
        var skillNode = new TextureButton
        {
            TextureNormal = new AtlasTexture
            {
                Atlas = texture,
                Region = new Rect2(unitSkill % 4 * texture.GetSize().x / 4, unitSkill / 4 * texture.GetSize().y / 7, texture.GetSize().x / 12, texture.GetSize().y / 7)
            },
            Expand = true,
            StretchMode = TextureButton.StretchModeEnum.KeepAspect,
            RectMinSize = Vector2.One * 50,
            HintTooltip = this.pluginUtils.FindSkill((Skill)unitSkill).Description
        };
        skillNode.Connect("pressed", this, nameof(RemoveSkillButtonPressed), new Godot.Collections.Array { skillNode });
        skillsContainer.AddChild(skillNode);
    }

    private void RemoveSkillButtonPressed(Node node)
    {
        var index = node.GetParent<Container>().GetChildren().IndexOf(node);
        this.Unit.Skills.RemoveAt(index);
        node.QueueFree();
        UpdateUnitDetails();
    }

    public void InitUnit(TransferConnectData.UnitData unit)
    {
        this.Unit = unit;
        var unitTypes = Enum.GetValues(typeof(UnitType)).Cast<UnitType>().ToList();
        for (var i = 0; i < unitTypes.Count; i++)
        {
            var unitType = unitTypes[i];
            unitTypeCombo.AddItem(unitType.ToString());
            if (unitType == unit.UnitType)
            {
                unitTypeCombo.Select(i);
            }
        }

        for (var j = 0; j < unit.Skills.Count; j++)
        {
            AddSkillButton((int)unit.Skills[j]);
        }

        UpdateUnitDetails();
    }

    private void UnitTypeChanged(int index)
    {
        this.Unit.UnitType = (UnitType)index;
        UpdateUnitDetails();
    }

    private void UpdateUnitDetails()
    {
        var unit = this.unitUtils.BuildUnit(new ServerPlayer(), this.Unit.UnitType, this.Unit.Skills);

        maxHP.Text = "MaxHP " + unit?.MaxHp.ToString() ?? "unknown";
        moveRange.Text = "Speed " + unit?.MoveDistance.ToString() ?? "unknown";
        sightRange.Text = "Vision " + unit?.SightRange.ToString() ?? "unknown";
        abilityContainer.ClearChildren();

        foreach (var ability in unit?.Abilities ?? new HashSet<Ability>())
        {
            var abilityNode = new TextureRect
            {
                Texture = ResourceLoader.Load<Texture>($"assets/Abilities/{ability}.png"),
                Expand = true,
                StretchMode = TextureRect.StretchModeEnum.KeepAspect,
                RectMinSize = Vector2.One * 50,
                HintTooltip = this.pluginUtils.FindAbility(ability).Description
            };

            abilityContainer.AddChild(abilityNode);
        }

        this.Visible = false;
        this.CallDeferred("set_visible", true);
    }
}
