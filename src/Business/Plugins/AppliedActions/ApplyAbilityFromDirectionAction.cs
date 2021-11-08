using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using System.Collections.Generic;

namespace IsometricGame.Logic.ScriptHelpers.AppliedActions
{
    public class ApplyAbilityFromDirectionAction : IAppliedAction
    {
        private readonly ServerUnit actionUnit;
        private readonly Ability ability;
        private readonly ServerUnit targetUnit;

        public ApplyAbilityFromDirectionAction(ServerUnit actionUnit, Ability ability, ServerUnit targetUnit)
        {
            this.actionUnit = actionUnit;
            this.ability = ability;
            this.targetUnit = targetUnit;
        }

        public void Apply(Dictionary<long, ServerTurnDelta> unitsTurnDelta)
        {
            var targetFullId = UnitUtils.GetFullUnitId(targetUnit);
            var targetDelta = unitsTurnDelta[targetFullId];
            targetDelta.AppliedAbilities.Add((ability, actionUnit.Position - targetUnit.Position));
        }
    }
}
