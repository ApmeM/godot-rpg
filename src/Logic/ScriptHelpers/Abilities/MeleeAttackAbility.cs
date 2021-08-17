using BrainAI.Pathfinding.BreadthFirst;
using Godot;
using IsometricGame.Logic.Models;
using IsometricGame.Logic.ScriptHelpers.Abilities.Action;
using System.Collections.Generic;

namespace IsometricGame.Logic.ScriptHelpers.Abilities
{
    public class MeleeAttackAbility : IAbility
    {
        public bool TargetUnit => false;

        public string Description => "Melee attack: \n Damage: 10.";

        public List<IAbilityAction> Apply(ServerUnit actionUnit, ServerUnit targetUnit)
        {
            return new List<IAbilityAction>
            {
                new ChangeHpAbilityAction(-(int)(actionUnit.AttackPower * 10)),
            };
        }
        public void HighliteMaze(Maze maze, Vector2 pos, ClientUnit currentUnit)
        {
            maze.HighliteAvailableAttacks(pos, 1, (int)currentUnit.AOEAttackRadius);
        }

        public bool IsApplicable(VectorGridGraph astar, ServerPlayer actionPlayer, ServerUnit actionUnit, ServerPlayer targetPlayer, ServerUnit targetUnit, Vector2 abilityDirection)
        {
            if (actionPlayer == targetPlayer)
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
    }
}
