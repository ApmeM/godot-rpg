using BrainAI.Pathfinding.AStar;
using BrainAI.Pathfinding.BreadthFirst;
using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using IsometricGame.Logic.ScriptHelpers.AppliedActions;
using System.Collections.Generic;
using System.Linq;

namespace IsometricGame.Logic.ScriptHelpers.Abilities
{
    public class MoveAbility : IMoveAbility
    {
        public AbilityType AbilityType => AbilityType.AreaOfEffect;

        public string Description => "Move: \n Distance: 10.";

        public Ability Ability => Ability.Move;

        public bool IsBasicMove => true;

        public void HighliteMaze(Maze maze, Vector2 oldPos, Vector2 newPos, ClientUnit currentUnit)
        {
            maze.BeginHighliting(Maze.HighliteType.Move, null);

            BreadthFirstPathfinder.Search(maze.astarMove, oldPos, currentUnit.MoveDistance.Value, out var visited);
            foreach (var cell in visited.Keys)
            {
                maze.HighlitePoint(cell);
            }

            maze.EndHighliting();
        }

        public List<IAppliedAction> Apply(ServerUnit actionUnit, GameData game, Vector2 abilityDirection)
        {
            var result = new List<IAppliedAction>();
            BreadthFirstPathfinder.Search(game.AstarMove, actionUnit.Position, actionUnit.MoveDistance, out var visited);
            if (!visited.ContainsKey(actionUnit.Position + abilityDirection))
            {
                return result;
            }

            var moveCells = BreadthFirstPathfinder.Search(game.AstarMove, actionUnit.Position, actionUnit.Position + abilityDirection)
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
            result.Add(new MoveAction(actionUnit, actionUnit.Position, this.Ability));

            return result;
        }

        public void MoveBy(Maze maze, Vector2 currentPosition, Vector2 newTarget, Queue<Vector2> currentPath)
        {
            var playerPosition = maze.WorldToMap(currentPosition);
            var newPath = AStarPathfinder.Search(maze.astarFly, playerPosition, newTarget);
            if (newPath == null)
            {
                return;
            }

            if (currentPath.Count > 0)
            {
                var current = currentPath.Peek();
                currentPath.Clear();
                currentPath.Enqueue(current);
            }

            for (var i = 0; i < newPath.Count; i++)
            {
                var worldPos = maze.GetSpritePositionForCell(newPath[i]);
                currentPath.Enqueue(worldPos);
            }
        }
    }
}
