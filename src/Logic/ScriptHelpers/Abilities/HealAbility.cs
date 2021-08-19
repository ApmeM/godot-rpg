using BrainAI.Pathfinding.BreadthFirst;
using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using IsometricGame.Logic.ScriptHelpers.AppliedActions;
using System.Collections.Generic;
using System.Linq;

namespace IsometricGame.Logic.ScriptHelpers.Abilities
{
    public class HealAbility : IAbility
    {
        public bool TargetUnit => true;

        public string Description => "Heal: \n Hp increase: 10.";

        public List<IAppliedAction> Apply(ServerUnit actionUnit, ServerUnit targetUnit)
        {
            return new List<IAppliedAction>
            {
                new ChangeHpAppliedAction(+(int)(actionUnit.MagicPower * 10), targetUnit),
            };
        }

        public void HighliteMaze(Maze maze, Vector2 pos, ClientUnit currentUnit)
        {
            var myUnits = maze.GetTree().GetNodesInGroup(Groups.MyUnits).Cast<Unit>();
            BreadthFirstPathfinder.Search(maze.astar, pos, (int)(currentUnit.RangedAttackDistance * 5), out var visited);
            var targetCells = myUnits
                .Select(unit => unit.NewPosition == null ? maze.WorldToMap(unit.Position) : unit.NewPosition.Value)
                .Where(visited.ContainsKey)
                .ToList();
            maze.HighliteAvailableAttacks(targetCells, (int)(currentUnit.AOEAttackRadius * 0));
        }

        public bool IsApplicable(VectorGridGraph astar, ServerPlayer actionPlayer, ServerUnit actionUnit, ServerPlayer targetPlayer, ServerUnit targetUnit, Vector2 abilityDirection)
        {
            if (actionPlayer != targetPlayer)
            {
                return false;
            }

            BreadthFirstPathfinder.Search(astar, actionUnit.Position, (int)(actionUnit.RangedAttackDistance * 5), out var visited);
            if (!visited.ContainsKey(actionUnit.Position + abilityDirection))
            {
                return false;
            }

            BreadthFirstPathfinder.Search(astar, actionUnit.Position + abilityDirection, (int)(actionUnit.AOEAttackRadius * 0), out visited);
            return visited.ContainsKey(targetUnit.Position);
        }
    }
}
