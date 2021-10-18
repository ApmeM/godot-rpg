using BrainAI.Pathfinding.BreadthFirst;
using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using IsometricGame.Logic.ScriptHelpers.AppliedActions;
using System.Collections.Generic;
using System.Linq;

namespace IsometricGame.Logic.ScriptHelpers.Abilities
{
    public class HasteAbility : IAbility
    {
        private readonly PluginUtils pluginUtils;

        public HasteAbility(PluginUtils pluginUtils)
        {
            this.pluginUtils = pluginUtils;
        }

        public bool TargetUnit => true;

        public string Description => $"Haste: \n Apply effect: {this.pluginUtils.FindEffect(Effect.Haste).Description}\n  Duration: 10.\n Cost: 5";

        public Ability Ability => Ability.Haste;

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

            if (actionUnit.Mp < 5)
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
                new ChangeMpAppliedAction(-5, actionUnit),
            };
        }

        public List<IAppliedAction> Apply(ServerUnit actionUnit, ServerUnit targetUnit)
        {
            return new List<IAppliedAction>
            {
                new ApplyEffectAppliedAction(Effect.Haste, 10, targetUnit),
            };
        }
    }
}
