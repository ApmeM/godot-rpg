using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using System.Collections.Generic;

namespace IsometricGame.Logic.ScriptHelpers.AppliedActions
{
    public class MoveAction : IAppliedAction
    {
        private readonly ServerUnit actionUnit;
        private readonly Vector2 movePosition;
        private readonly Ability moveAbility;

        public MoveAction(ServerUnit actionUnit, Vector2 movePosition, Ability moveAbility)
        {
            this.actionUnit = actionUnit;
            this.movePosition = movePosition;
            this.moveAbility = moveAbility;
        }

        public void Apply(Dictionary<long, ServerTurnDelta> unitsTurnDelta)
        {
            var actionFullId = UnitUtils.GetFullUnitId(actionUnit);
            var actionDelta = unitsTurnDelta[actionFullId];
            actionDelta.MoveAbilityUsed = moveAbility;
        }
    }
}
