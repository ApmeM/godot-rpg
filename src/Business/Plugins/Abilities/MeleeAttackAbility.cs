using BrainAI.Pathfinding.BreadthFirst;
using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using IsometricGame.Logic.ScriptHelpers.AppliedActions;
using System.Collections.Generic;

namespace IsometricGame.Logic.ScriptHelpers.Abilities
{
    public class MeleeAttackAbility : IAbility
    {
        public bool TargetUnit => false;

        public string Description => "Melee attack: \n Damage: 10.";

        public Ability Ability => Ability.MeleeAttack;

        public void HighliteMaze(Maze maze, Vector2 pos, ClientUnit currentUnit)
        {
            maze.HighliteAvailableAttacks(pos, 1, (int)currentUnit.AOEAttackRadius);
        }

        public bool IsApplicable(MapGraphData astar, ServerUnit actionUnit, ServerUnit targetUnit, Vector2 abilityDirection)
        {
            if (actionUnit.Player == targetUnit.Player)
            {
                return false;
            }
            
            BreadthFirstPathfinder.Search(astar, actionUnit.Position, 1, out var visited);
            if (!visited.ContainsKey(actionUnit.Position + abilityDirection))
            {
                return false;
            }

            BreadthFirstPathfinder.Search(astar, actionUnit.Position + abilityDirection, (int)(actionUnit.AOEAttackRadius), out visited);
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
                new ChangeHpAppliedAction(-(int)(actionUnit.AttackPower * 10), targetUnit),
            };
        }
    }
}
