using BrainAI.Pathfinding.BreadthFirst;
using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using System.Collections.Generic;
using System.Linq;

namespace IsometricGame.Logic.ScriptHelpers.Abilities
{
    public class MoveAbility : IAbility
    {
        public bool TargetUnit => false;

        public string Description => "Move: \n Distance: 10.";

        public Ability Ability => Ability.Move;

        public void HighliteMaze(Maze maze, Vector2 pos, ClientUnit currentUnit)
        {
            maze.HighliteAvailableAttacks(pos, 1, (int)currentUnit.MoveDistance);
        }

        public List<IAppliedAction> Apply(ServerUnit actionUnit, GameData game, Vector2 abilityDirection)
        {
            var result = new List<IAppliedAction>();
            if (actionUnit.Hp == 0)
            {
                return result;
            }

            BreadthFirstPathfinder.Search(game.Astar, actionUnit.Position, actionUnit.MoveDistance, out var visited);
            if (!visited.ContainsKey(actionUnit.Position + abilityDirection))
            {
                return result;
            }

            var moveCells = BreadthFirstPathfinder.Search(game.Astar, actionUnit.Position, actionUnit.Position + abilityDirection)
                .Take(actionUnit.MoveDistance + 1)
                .Select((a, b) => new { a, b })
                .ToDictionary(a => a.a, a => a.b);


            foreach (var targetPlayer in game.Players)
            {
                foreach (var targetUnit in targetPlayer.Value.Units)
                {
                    moveCells.Remove(targetUnit.Value.Position);
                }
            }

            if (!moveCells.Any())
            {
                return result;
            }

            actionUnit.Position = moveCells.OrderByDescending(a => a.Value).First().Key;

            return result;
        }
    }
}
