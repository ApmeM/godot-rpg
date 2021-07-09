using Godot;
using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.Abilities
{
    public class MeleeAttackAbility : IAbility
    {
        public void Apply(ServerUnit actionUnit, ServerUnit targetUnit)
        {
            targetUnit.Hp -= (int)(actionUnit.AttackPower * 10);
        }

        public void HighliteMaze(Maze maze, Vector2 pos, ClientUnit currentUnit)
        {
            maze.HighliteAvailableAttacks(pos, 1, (int)currentUnit.AOEAttackRadius);
        }

        public bool IsApplicable(ServerPlayer actionPlayer, ServerUnit actionUnit, ServerPlayer targetPlayer, ServerUnit targetUnit)
        {
            return actionPlayer != targetPlayer && targetUnit.Hp > 0;
        }

        public bool IsInRange(ServerUnit actionUnit, ServerUnit targetUnit, Vector2 abilityDirection)
        {
            return IsometricMove.Distance(targetUnit.Position, actionUnit.Position + abilityDirection) <= (int)actionUnit.AOEAttackRadius;
        }
    }
}
