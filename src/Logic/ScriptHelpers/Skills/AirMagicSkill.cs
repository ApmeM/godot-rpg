using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.Skills
{
    public class AirMagicSkill : ISkill
    {
        public string Description => $"Air magic: Add Ability: \n{UnitUtils.FindAbility(Ability.Haste).Description}";

        public void Apply(ServerPlayer player, ServerUnit unit)
        {
            unit.Abilities.Add(Ability.Haste);
        }
    }
}
