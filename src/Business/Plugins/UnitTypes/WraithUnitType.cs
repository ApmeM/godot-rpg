using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.Skills
{
    public class WraithUnitType : IUnitType
    {
        public UnitType UnitType => UnitType.Wraith;

        public void Apply(ServerUnit unit)
        {
            unit.Abilities.Add(Ability.ImmaterialMove);
        }

        public void Initialize(ServerUnit unit)
        {
        }
    }
}
