using Godot;
using IsometricGame.Logic;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using IsometricGame.Logic.ScriptHelpers;
using IsometricGame.Logic.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

public class TeamSelectUnit : VBoxContainer
{
    private TransferConnectData.UnitData Unit;

    private Label maxHpLabel;
    private Label moveRangeLabel;
    private Label sightRangeLabel;
    private Container abilityContainer;
    private Container skillsContainer;
    private OptionButton unitTypeCombo;
    private WindowDialog windowDialog;
    private Container availableSkillsContainer;
    
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

        this.GetNode<Button>("ManageSkillsButton").Connect("pressed", this, nameof(ManageSkillButtonPressed));

        maxHpLabel = GetNode<Label>("MaxHP");
        moveRangeLabel = GetNode<Label>("MoveRange");
        sightRangeLabel = GetNode<Label>("SightRange");
        abilityContainer = GetNode<Container>("AbilityContainer");
        skillsContainer = this.GetNode<Container>("SkillsContainer");
        unitTypeCombo = this.GetNode<OptionButton>("UnitTypeCombo");
        windowDialog = this.GetNode<WindowDialog>("WindowDialog");
        availableSkillsContainer = windowDialog.GetNode<Container>("AvailableSkillsContainer");

        unitTypeCombo.Connect("item_selected", this, nameof(UnitTypeChanged));

        var skills = Enum.GetValues(typeof(Skill)).Cast<Skill>().ToList();
        var texture = ResourceLoader.Load<Texture>("assets/Skills.png");
        ClearContainer(availableSkillsContainer);
        ClearContainer(skillsContainer);
        ClearContainer(abilityContainer);
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

        maxHpLabel.Text = "MaxHP " + unit?.MaxHp.ToString() ?? "unknown";
        moveRangeLabel.Text = "Speed " + unit?.MoveDistance.ToString() ?? "unknown";
        sightRangeLabel.Text = "Vision " + unit?.SightRange.ToString() ?? "unknown";
        ClearContainer(abilityContainer);

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

    private void ClearContainer(Container container)
    {
        foreach (Node node in container.GetChildren())
        {
            node.QueueFree();
        }
    }
}
