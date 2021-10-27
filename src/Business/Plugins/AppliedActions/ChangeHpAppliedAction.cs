using IsometricGame.Logic.Models;
using System.Collections.Generic;

namespace IsometricGame.Logic.ScriptHelpers.AppliedActions
{
    public class ChangeHpAppliedAction : IAppliedAction
    {
        private readonly ServerUnit unit;
        private readonly int value;

        public ChangeHpAppliedAction(int value, ServerUnit unit)
        {
            this.value = value;
            this.unit = unit;
        }

        public void Apply(Dictionary<long, ServerTurnDelta> unitsTurnDelta)
        {
            this.unit.Hp += value;
            unitsTurnDelta[UnitUtils.GetFullUnitId(this.unit)].HpChanges.Add(value);
        }
    }
}
