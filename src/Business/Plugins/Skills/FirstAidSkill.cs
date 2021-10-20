using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.Skills
{
    public class FirstAidSkill : ISkill
    {
        private readonly PluginUtils pluginUtils;

        public FirstAidSkill(PluginUtils pluginUtils)
        {
            this.pluginUtils = pluginUtils;
        }

        public string Description => $"First aid: Add Ability: \n{this.pluginUtils.FindAbility(Ability.Heal).Description}";

        public Skill Skill => Skill.FirstAid;

        public void Apply(ServerPlayer player, ServerUnit unit)
        {
            unit.Abilities.Add(Ability.Heal);
        }
    }
}
