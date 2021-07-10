using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.Skills
{
    public class AmazonUnitType : IUnitType
    {
        public void Apply(ServerUnit unit)
        {
            unit.SightRange = 7;
            unit.Abilities.Add(Ability.RangedAttack);
        }

        public void Initialize(ServerUnit unit)
        {
        }
    }
}
