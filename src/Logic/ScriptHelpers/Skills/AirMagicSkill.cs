using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.Skills
{
    public class AirMagicSkill : ISkill
    {
        public void Apply(ServerPlayer player, ServerUnit unit)
        {
            unit.Abilities.Add(Ability.Haste);
        }
    }
}
