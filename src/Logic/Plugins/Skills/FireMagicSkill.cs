using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.Skills
{
    public class FireMagicSkill : ISkill
    {
        private readonly PluginUtils pluginUtils;

        public FireMagicSkill(PluginUtils pluginUtils)
        {
            this.pluginUtils = pluginUtils;
        }

        public string Description => $"Fire magic: Add Ability: \n{this.pluginUtils.FindAbility(Ability.Fireball).Description}";

        public Skill Skill => Skill.FireMagic;

        public void Apply(ServerPlayer player, ServerUnit unit)
        {
            unit.Abilities.Add(Ability.Fireball);
        }
    }
}
