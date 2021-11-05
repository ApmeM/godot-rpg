using Godot;
using IsometricGame.Logic.Models;
using System.Collections.Generic;

namespace IsometricGame.Logic.ScriptHelpers.AppliedActions
{
    public class ApplyAbilityToDirectionAction : IAppliedAction
    {
        private readonly ServerUnit actionUnit;
        private readonly Vector2 abilityDirection;

        public ApplyAbilityToDirectionAction(ServerUnit actionUnit, Vector2 abilityDirection)
        {
            this.actionUnit = actionUnit;
            this.abilityDirection = abilityDirection;
        }

        public void Apply(Dictionary<long, ServerTurnDelta> unitsTurnDelta)
        {
            var actionFullId = UnitUtils.GetFullUnitId(actionUnit);
            var actionDelta = unitsTurnDelta[actionFullId];
            actionDelta.AbilityDirection = abilityDirection;
        }
    }
}
