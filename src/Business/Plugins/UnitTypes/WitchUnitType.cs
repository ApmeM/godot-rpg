using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.Skills
{
    public class WitchUnitType : IUnitType
    {
        public UnitType UnitType => UnitType.Witch;

        public void Apply(ServerUnit unit)
        {
            unit.Abilities.Add(Ability.Fly);
            unit.Abilities.Add(Ability.HasteAura);
            unit.Abilities.Add(Ability.Regeneration);
        }

        public void Initialize(ServerUnit unit)
        {
        }
    }
}
