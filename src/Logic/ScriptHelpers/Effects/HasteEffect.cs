using IsometricGame.Logic.Models;
using IsometricGame.Logic.ScriptHelpers.AppliedActions;
using System.Collections.Generic;

namespace IsometricGame.Logic.ScriptHelpers.Effects
{
    public class HasteEffect : IEffect
    {
        public string Description => "Haste effect: \n  Speed: x1.5 times.";

        public List<IAppliedAction> Apply(ServerUnit unit)
        {
            return new List<IAppliedAction>
            {
                new ChangeMoveDistanceAppliedAction((int)(unit.MoveDistance * 0.5f), unit)
            };
        }
    }
}
