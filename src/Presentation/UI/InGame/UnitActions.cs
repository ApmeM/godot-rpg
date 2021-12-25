using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.ScriptHelpers;
using IsometricGame.Logic.Utils;
using IsometricGame.Presentation;
using IsometricGame.Presentation.Utils;
using System.Collections.Generic;

[SceneReference("UnitActions.tscn")]
public partial class UnitActions : Control
{
    [Signal]
    public delegate void ActionSelected(CurrentAction action, Ability ability);

    private readonly PluginUtils pluginUtils;
    
    public UnitActions()
    {
        this.pluginUtils = DependencyInjector.pluginUtils;
    }

    public List<Ability> Abilities
    {
        set
        {
            abilityContainer.ClearChildren();

            for (var i = 0; i < value.Count; i++)
            {
                var ability = value[i];
                var abilityNode = new TextureButton
                {
                    TextureNormal = ResourceLoader.Load<Texture>($"assets/Abilities/{ability}.png"),
                    Expand = true,
                    StretchMode = TextureButton.StretchModeEnum.KeepAspect,
                    RectMinSize = Vector2.One * 24,
                    HintTooltip = this.pluginUtils.FindAbility(ability).Description
                };

                abilityContainer.AddChild(abilityNode);
                abilityNode.Connect("pressed", this, nameof(AttackButtonPressed), new Godot.Collections.Array { ability });
            }

            this.Visible = false;
            this.CallDeferred("set_visible", true);
        }
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
    }

    private void AttackButtonPressed(Ability ability)
    {
        EmitSignal(nameof(ActionSelected), CurrentAction.UseAbility, ability);
    }
}
