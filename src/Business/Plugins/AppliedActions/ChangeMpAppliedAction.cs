using IsometricGame.Logic.Models;
using System.Collections.Generic;

namespace IsometricGame.Logic.ScriptHelpers.AppliedActions
{
    public class ChangeMpAppliedAction : IAppliedAction
    {
        private readonly ServerUnit unit;
        private readonly int value;

        public ChangeMpAppliedAction(int value, ServerUnit unit)
        {
            this.value = value;
            this.unit = unit;
        }

        public void Apply(Dictionary<long, ServerTurnDelta> unitsTurnDelta)
        {
            this.unit.Mp += value;
            unitsTurnDelta[UnitUtils.GetFullUnitId(this.unit)].MpChanges.Add(value);
        }
    }
}
