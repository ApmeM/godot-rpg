using BrainAI.Pathfinding.BreadthFirst;
using Godot;
using IsometricGame.Logic.Models;
using IsometricGame.Logic.ScriptHelpers.AppliedActions;
using System.Collections.Generic;

namespace IsometricGame.Logic.ScriptHelpers.Abilities
{
    public class RangedAttackAbility : IAbility
    {
        public bool TargetUnit => false;

        public string Description => "Ranged attack: \n Damage: 5.";

        public void HighliteMaze(Maze maze, Vector2 pos, ClientUnit currentUnit)
        {
            maze.HighliteAvailableAttacks(pos, (int)(currentUnit.RangedAttackDistance * 5), (int)(currentUnit.AOEAttackRadius * 2));
        }

        public bool IsApplicable(VectorGridGraph astar, ServerUnit actionUnit, ServerUnit targetUnit, Vector2 abilityDirection)
        {
            if (actionUnit.Player == targetUnit.Player)
            {
                return false;
            }
            
            BreadthFirstPathfinder.Search(astar, actionUnit.Position, (int)(actionUnit.RangedAttackDistance * 5), out var visited);
            if (!visited.ContainsKey(actionUnit.Position + abilityDirection))
            {
                return false;
            }

            BreadthFirstPathfinder.Search(astar, actionUnit.Position + abilityDirection, (int)(actionUnit.AOEAttackRadius * 2), out visited);
            return visited.ContainsKey(targetUnit.Position);
        }

        public List<IAppliedAction> ApplyCost(ServerUnit actionUnit)
        {
            return new List<IAppliedAction>();
        }

        public List<IAppliedAction> Apply(ServerUnit actionUnit, ServerUnit targetUnit)
        {
            return new List<IAppliedAction>
            {
                new ChangeHpAppliedAction(-(int)(actionUnit.AttackPower * 5), targetUnit),
            };
        }
    }
}
