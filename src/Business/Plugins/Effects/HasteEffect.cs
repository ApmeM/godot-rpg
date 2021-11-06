using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using IsometricGame.Logic.ScriptHelpers.AppliedActions;
using System.Collections.Generic;

namespace IsometricGame.Logic.ScriptHelpers.Effects
{
    public class HasteEffect : IEffect
    {
        public string Description => "Haste effect: \n  Speed: 3.";

        public Effect Effect => Effect.Haste;

        public List<IAppliedAction> Apply(ServerUnit unit)
        {
            return new List<IAppliedAction>
            {
                new ChangeMoveDistanceAppliedAction(3, unit)
            };
        }
    }
}
