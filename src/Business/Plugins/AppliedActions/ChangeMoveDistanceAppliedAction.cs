using IsometricGame.Logic.Models;
using System.Collections.Generic;

namespace IsometricGame.Logic.ScriptHelpers.AppliedActions
{
    public class ChangeMoveDistanceAppliedAction : IAppliedAction
    {
        private readonly int value;
        private readonly ServerUnit unit;

        public ChangeMoveDistanceAppliedAction(int value, ServerUnit unit)
        {
            this.value = value;
            this.unit = unit;
        }

        public void Apply(Dictionary<long, ServerTurnDelta> unitsTurnDelta)
        {
            this.unit.MoveDistance += value;
        }
    }
}
