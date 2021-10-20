using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.Skills
{
    public class GoatmanUnitType : IUnitType
    {
        public UnitType UnitType => UnitType.Goatman;

        public void Apply(ServerUnit unit)
        {
            unit.MaxHp = 20;
            unit.Abilities.Add(Ability.MeleeAttack);
        }

        public void Initialize(ServerUnit unit)
        {
            unit.Hp = 20;
        }
    }
}
