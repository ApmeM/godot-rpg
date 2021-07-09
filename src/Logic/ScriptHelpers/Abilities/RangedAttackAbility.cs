using Godot;
using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.Abilities
{
    public class RangedAttackAbility : IAbility
    {
        public void Apply(ServerUnit actionUnit, ServerUnit targetUnit)
        {
            targetUnit.Hp -= (int)(actionUnit.AttackPower * 5);
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
