using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.Skills
{
    public class HealUsable : IUsable
    {
        public Usable Name => Usable.Heal;

        public void Apply(ServerUnit actionUnit, ServerUnit targetUnit)
        {
            targetUnit.Hp += actionUnit.AttackDamage;
        }

        public void HighliteMaze(Maze maze, Vector2 pos, ClientUnit currentUnit)
        {
            maze.HighliteAvailableAttacks(pos, currentUnit.AttackDistance, currentUnit.AttackRadius);
        }

        public bool IsApplicable(ServerPlayer actionPlayer, ServerUnit actionUnit, ServerPlayer targetPlayer, ServerUnit targetUnit)
        {
            return actionPlayer == targetPlayer && targetUnit.Hp < targetUnit.MaxHp;
        }

        public bool IsInRange(ServerUnit actionUnit, ServerUnit targetUnit, Vector2 usableDirection)
        {
            return IsometricMove.Distance(targetUnit.Position, actionUnit.Position + usableDirection) <= actionUnit.AttackRadius;
        }
    }
}
