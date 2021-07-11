using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using System.Linq;

namespace IsometricGame.Logic.ScriptHelpers.Abilities
{
    public class FireballAbility : IAbility
    {
        public void Apply(ServerUnit actionUnit, ServerUnit targetUnit)
        {
            targetUnit.Hp -= (int)(actionUnit.MagicPower * 2);

            var effect = targetUnit.Effects.FirstOrDefault(a => a.Effect == Effect.Burn);
            if (effect == null)
            {
                effect = new EffectDuration { Effect = Effect.Burn };
                targetUnit.Effects.Add(effect);
            }

            effect.Duration = 5;
        }

        public void HighliteMaze(Maze maze, Vector2 pos, ClientUnit currentUnit)
        {
            maze.HighliteAvailableAttacks(pos, (int)(currentUnit.RangedAttackDistance * 5), (int)(currentUnit.AOEAttackRadius * 2));
        }

        public bool IsApplicable(ServerPlayer actionPlayer, ServerUnit actionUnit, ServerPlayer targetPlayer, ServerUnit targetUnit)
        {
            return actionPlayer != targetPlayer && targetUnit.Hp > 0;
        }

        public bool IsInRange(ServerUnit actionUnit, ServerUnit targetUnit, Vector2 abilityDirection)
        {
            return IsometricMove.Distance(targetUnit.Position, actionUnit.Position + abilityDirection) <= (int)(actionUnit.RangedAttackDistance * 5);
        }
    }
}
