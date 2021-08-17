using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.Skills
{
    public class FirstAidSkill : ISkill
    {
        public string Description => $"First aid: Add Ability: \n{UnitUtils.FindAbility(Ability.Heal).Description}";
        public void Apply(ServerPlayer player, ServerUnit unit)
        {
            unit.Abilities.Add(Ability.Heal);
        }
    }
}
