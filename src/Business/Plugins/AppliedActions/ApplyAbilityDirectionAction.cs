using IsometricGame.Logic.Models;
using System.Collections.Generic;

namespace IsometricGame.Logic.ScriptHelpers.AppliedActions
{
    public class ApplyAbilityDirectionAction : IAppliedAction
    {
        private readonly ServerUnit actionUnit;
        private readonly ServerUnit targetUnit;

        public ApplyAbilityDirectionAction(ServerUnit actionUnit, ServerUnit targetUnit)
        {
            this.actionUnit = actionUnit;
            this.targetUnit = targetUnit;
        }

        public void Apply(Dictionary<long, ServerTurnDelta> unitsTurnDelta)
        {
            var targetFullId = UnitUtils.GetFullUnitId(targetUnit);
            var targetDelta = unitsTurnDelta[targetFullId];
            targetDelta.AbilityFrom = actionUnit.Position - targetUnit.Position;
        }
    }
}
