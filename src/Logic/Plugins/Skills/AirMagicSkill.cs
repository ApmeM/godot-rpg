using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.Skills
{
    public class AirMagicSkill : ISkill
    {
        private readonly PluginUtils pluginUtils;

        public AirMagicSkill(PluginUtils pluginUtils)
        {
            this.pluginUtils = pluginUtils;
        }

        public string Description => $"Air magic: Add Ability: \n{this.pluginUtils.FindAbility(Ability.Haste).Description}";

        public Skill Skill => Skill.AirMagic;

        public void Apply(ServerPlayer player, ServerUnit unit)
        {
            unit.Abilities.Add(Ability.Haste);
        }
    }
}
