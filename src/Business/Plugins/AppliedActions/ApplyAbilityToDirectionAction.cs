using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using System;
using System.Collections.Generic;

namespace IsometricGame.Logic.ScriptHelpers.AppliedActions
{
    public class ApplyAbilityToDirectionAction : IAppliedAction
    {
        private readonly ServerUnit actionUnit;
        private readonly Ability ability;
        private readonly Vector2 abilityDirection;

        public ApplyAbilityToDirectionAction(ServerUnit actionUnit, Ability ability, Vector2 abilityDirection)
        {
            this.actionUnit = actionUnit;
            this.ability = ability;
            this.abilityDirection = abilityDirection;
        }

        public void Apply(Dictionary<long, ServerTurnDelta> unitsTurnDelta)
        {
            var actionFullId = UnitUtils.GetFullUnitId(actionUnit);
            var actionDelta = unitsTurnDelta[actionFullId];
            actionDelta.ExecutedAbilities.Add(new ValueTuple<Ability, Vector2>(ability, abilityDirection));
        }
    }
}
