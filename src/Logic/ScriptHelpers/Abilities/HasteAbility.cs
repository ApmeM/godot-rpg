using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.Abilities
{
    public class HasteAbility : IAbility
    {
        public void Apply(ServerUnit actionUnit, ServerUnit targetUnit)
        {
            targetUnit.Effects.Add(new EffectDuration { Effect = Effect.Haste, Duration = 10 });
        }

        public void HighliteMaze(Maze maze, Vector2 pos, ClientUnit currentUnit)
        {
            maze.HighliteAvailableAttacks(pos, (int)(currentUnit.RangedAttackDistance * 5), 0);
        }

        public bool IsApplicable(ServerPlayer actionPlayer, ServerUnit actionUnit, ServerPlayer targetPlayer, ServerUnit targetUnit)
        {
            return actionPlayer == targetPlayer && targetUnit.Hp > 0;
        }

        public bool IsInRange(ServerUnit actionUnit, ServerUnit targetUnit, Vector2 abilityDirection)
        {
            var distance = IsometricMove.Distance(targetUnit.Position, actionUnit.Position + abilityDirection);
            return distance <= 0;
        }
    }
}
