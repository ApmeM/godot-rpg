using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using System.Linq;

namespace IsometricGame.Logic.ScriptHelpers.Abilities.Action
{
    public class ApplyEffectAbilityAction : IAbilityAction
    {
        private readonly Effect effect;
        private readonly int duration;

        public ApplyEffectAbilityAction(Effect effect, int duration)
        {
            this.effect = effect;
            this.duration = duration;
        }

        public void Apply(ServerUnit unit)
        {
            var existingEffect = unit.Effects.FirstOrDefault(a => a.Effect == effect);
            if (existingEffect == null)
            {
                existingEffect = new EffectDuration { Effect = effect };
                unit.Effects.Add(existingEffect);
            }

            existingEffect.Duration = duration;
        }
    }
}
