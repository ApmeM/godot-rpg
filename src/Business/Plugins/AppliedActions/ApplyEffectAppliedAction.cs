using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using System.Collections.Generic;
using System.Linq;

namespace IsometricGame.Logic.ScriptHelpers.AppliedActions
{
    public class ApplyEffectAppliedAction : IAppliedAction
    {
        private readonly Effect effect;
        private readonly int duration;
        private readonly ServerUnit unit;

        public ApplyEffectAppliedAction(Effect effect, int duration, ServerUnit unit)
        {
            this.effect = effect;
            this.duration = duration;
            this.unit = unit;
        }

        public void Apply(Dictionary<long, ServerTurnDelta> unitsTurnDelta)
        {
            var existingEffect = this.unit.Effects.FirstOrDefault(a => a.Effect == effect);
            if (existingEffect == null)
            {
                existingEffect = new EffectDuration { Effect = effect };
                this.unit.Effects.Add(existingEffect);
            }

            existingEffect.Duration = duration;
        }
    }
}
