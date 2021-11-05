using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using IsometricGame.Logic.ScriptHelpers;
using IsometricGame.Logic.Utils;
using IsometricGame.Presentation;
using System.Collections.Generic;

[SceneReference("UnitDetails.tscn")]
public partial class UnitDetails : VBoxContainer
{
    private readonly PluginUtils pluginUtils;

    public UnitDetails()
    {
        this.pluginUtils = DependencyInjector.pluginUtils;
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }

    public void SelectUnit(ClientUnit unit)
    {
        maxHP.Text = "MaxHP " + unit?.MaxHp.ToString() ?? "unknown";
        moveRange.Text = "Speed " + unit?.MoveDistance.ToString() ?? "unknown";
        sightRange.Text = "Vision " + unit?.SightRange.ToString() ?? "unknown";

        var texture = ResourceLoader.Load<Texture>("assets/Skills.png");
        foreach (Node node in skillsContainer.GetChildren())
        {
            node.QueueFree();
        }

        foreach (var skill in unit?.Skills ?? new HashSet<Skill>())
        {
            var unitSkill = (int)skill;
            var skillNode = new TextureRect
            {
                Texture = new AtlasTexture
                {
                    Atlas = texture,
                    Region = new Rect2(unitSkill % 4 * texture.GetSize().x / 4, unitSkill / 4 * texture.GetSize().y / 7, texture.GetSize().x / 12, texture.GetSize().y / 7)
                },
                Expand = true,
                StretchMode = TextureRect.StretchModeEnum.KeepAspect,
                RectMinSize = Vector2.One * 50,
                HintTooltip = this.pluginUtils.FindSkill((Skill)unitSkill).Description
            };

            skillsContainer.AddChild(skillNode);
        }
        
        foreach (Node node in effectsContainer.GetChildren())
        {
            node.QueueFree();
        }

        foreach (var effect in unit?.Effects ?? new List<EffectDuration>())
        {
            var container = new CenterContainer();
            var effectNode = new TextureRect
            {
                Texture = ResourceLoader.Load<Texture>($"assets/Effects/{effect.Effect}.png"),
                Expand = true,
                StretchMode = TextureRect.StretchModeEnum.KeepAspect,
                RectMinSize = Vector2.One * 50,
                HintTooltip = this.pluginUtils.FindEffect(effect.Effect).Description,
            };
            container.AddChild(effectNode);
            if (effect.Duration > 0)
            {
                var durationNode = new Label
                {
                    Text = effect.Duration.ToString()
                };
                container.AddChild(durationNode);
            }
            else
            {
                effectNode.Material = ResourceLoader.Load<Material>($"Presentation/Shaders/Grayscale.tres");
            }

            effectsContainer.AddChild(container);
        }

        this.Visible = false;
        this.CallDeferred("set_visible", true);
    }
}
