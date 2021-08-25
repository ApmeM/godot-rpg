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

        public string Description => "Heal: \n Hp increase: 10.\n Cost: 2";

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

        public bool IsApplicable(VectorGridGraph astar, ServerUnit actionUnit, ServerUnit targetUnit, Vector2 abilityDirection)
        {
            if (actionUnit.Player != targetUnit.Player)
            {
                return false;
            }

            if (actionUnit.Mp < 2)
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

        public List<IAppliedAction> ApplyCost(ServerUnit actionUnit)
        {
            return new List<IAppliedAction>
            {
                new ChangeMpAppliedAction(-2, actionUnit),
            };
        }

        public List<IAppliedAction> Apply(ServerUnit actionUnit, ServerUnit targetUnit)
        {
            return new List<IAppliedAction>
            {
                new ChangeHpAppliedAction(+(int)(actionUnit.MagicPower * 10), targetUnit),
            };
        }
    }
}
