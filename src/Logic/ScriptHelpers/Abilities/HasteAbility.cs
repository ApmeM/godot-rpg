using BrainAI.Pathfinding.BreadthFirst;
using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using System.Linq;

namespace IsometricGame.Logic.ScriptHelpers.Abilities
{
    public class HasteAbility : IAbility
    {
        public bool TargetUnit => false;

        public void Apply(ServerUnit actionUnit, ServerUnit targetUnit)
        {
            var effect = targetUnit.Effects.FirstOrDefault(a => a.Effect == Effect.Haste);
            if (effect == null)
            {
                effect = new EffectDuration { Effect = Effect.Haste };
                targetUnit.Effects.Add(effect);
            }

            effect.Duration = 10;
        }

        public void HighliteMaze(Maze maze, Vector2 pos, ClientUnit currentUnit)
        {
            maze.HighliteAvailableAttacks(pos, (int)(currentUnit.RangedAttackDistance * 5), 0);
        }


        public bool IsApplicable(VectorGridGraph astar, ServerPlayer actionPlayer, ServerUnit actionUnit, ServerPlayer targetPlayer, ServerUnit targetUnit, Vector2 abilityDirection)
        {
            if(actionPlayer != targetPlayer || targetUnit.Hp <= 0)
            {
                return false;
            }    
        
            BreadthFirstPathfinder.Search(astar, actionUnit.Position, (int)(actionUnit.RangedAttackDistance * 5), out var visited);
            if (!visited.ContainsKey(actionUnit.Position + abilityDirection))
            {
                return false;
            }

            BreadthFirstPathfinder.Search(astar, actionUnit.Position + abilityDirection, 0, out visited);
            return visited.ContainsKey(targetUnit.Position);
        }
    }
}
