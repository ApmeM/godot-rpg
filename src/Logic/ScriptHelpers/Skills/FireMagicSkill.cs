using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.Skills
{
    public class FireMagicSkill : ISkill
    {
        public string Description => $"Fire magic: Add Ability: \n{UnitUtils.FindAbility(Ability.Fireball).Description}";

        public void Apply(ServerPlayer player, ServerUnit unit)
        {
            unit.Abilities.Add(Ability.Fireball);
        }
    }
}
