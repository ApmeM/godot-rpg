using BrainAI.Pathfinding.BreadthFirst;
using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using IsometricGame.Logic.ScriptHelpers.AppliedActions;
using System.Collections.Generic;

namespace IsometricGame.Logic.ScriptHelpers.Abilities
{
    public class FireballAbility : IAbility
    {
        public bool TargetUnit => false;

        public string Description => $"Fireball: \n Direct Damage: 2. \n Apply effect: { UnitUtils.FindEffect(Effect.Burn).Description } \n  Duration: 5\n Cost: 5";

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

            if (actionUnit.Mp < 5)
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
            return new List<IAppliedAction>
            {
                new ChangeMpAppliedAction(-5, actionUnit),
            };
        }

        public List<IAppliedAction> Apply(ServerUnit actionUnit, ServerUnit targetUnit)
        {
            return new List<IAppliedAction>
            {
                new ChangeHpAppliedAction(-(int)(actionUnit.MagicPower * 2), targetUnit),
                new ApplyEffectAppliedAction(Effect.Burn, 5, targetUnit),
            };
        }
    }
}
