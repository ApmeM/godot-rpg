using IsometricGame.Logic.Models;
using IsometricGame.Logic.ScriptHelpers.AppliedActions;
using System.Collections.Generic;

namespace IsometricGame.Logic.ScriptHelpers.Effects
{
    public class BurnEffect : IEffect
    {
        public string Description => "Burn effect: \n  Damage: 1";

        public List<IAppliedAction> Apply(ServerUnit unit)
        {
            return new List<IAppliedAction>
            {
                new ChangeHpAppliedAction(-1, unit)
            };
        }
    }
}
