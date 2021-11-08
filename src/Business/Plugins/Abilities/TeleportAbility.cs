using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using IsometricGame.Logic.ScriptHelpers.AppliedActions;
using System.Collections.Generic;

namespace IsometricGame.Logic.ScriptHelpers.Abilities
{
    public class TeleportAbility : IMoveAbility
    {
        private const int TeleportDistance = 3;

        public AbilityType AbilityType => AbilityType.AreaOfEffect;

        public string Description => $"Teleport: \n Distance: {TeleportDistance}. Cost 5";

        public Ability Ability => Ability.Teleport;

        public bool IsBasicMove => false;

        public void HighliteMaze(Maze maze, Vector2 oldPos, Vector2 newPos, ClientUnit currentUnit)
        {
            maze.BeginHighliting(Maze.HighliteType.Move, null);

            HashSet<Vector2> checkedPositions = new HashSet<Vector2> { newPos };
            HashSet<Vector2> nextPositions = new HashSet<Vector2>();
            var visited = new Dictionary<Vector2, Vector2> { { newPos, newPos } };
            for (var i = 0; i < TeleportDistance; i++)
            {
                foreach (var pos in checkedPositions)
                {
                    foreach (var dir in MapGraphData.CardinalDirs)
                    {
                        if (visited.ContainsKey(pos + dir))
                        {
                            continue;
                        }

                        if (maze.astarMove.Paths.Contains(pos + dir))
                        {
                            visited[pos + dir] = pos;
                        }

                        nextPositions.Add(pos + dir);
                    }
                }

                var tmp = checkedPositions;
                checkedPositions = nextPositions;
                nextPositions = tmp;
                nextPositions.Clear();
            }

            foreach (var cell in visited.Keys)
            {
                maze.HighlitePoint(cell);
            }

            maze.EndHighliting();

        }

        public List<IAppliedAction> Apply(ServerUnit actionUnit, GameData game, Vector2 abilityDirection)
        {
            var result = new List<IAppliedAction>();
            if (actionUnit.Mp < 5)
            {
                return result;
            }

            if (abilityDirection.x + abilityDirection.y > 3)
            {
                return result;
            }

            var newPosition = actionUnit.Position + abilityDirection;
            if (!game.AstarMove.Paths.Contains(newPosition))
            {
                return result;
            }


            foreach (var targetPlayer in game.Players)
            {
                foreach (var targetUnit in targetPlayer.Value.Units)
                {
                    if (newPosition == targetUnit.Value.Position)
                    {
                        return result;
                    }
                }
            }

            result.Add(new ChangeMpAppliedAction(-5, actionUnit));
            result.Add(new MoveAction(actionUnit, newPosition, this.Ability));

            return result;
        }

        public void MoveBy(Maze maze, Vector2 currentPosition, Vector2 newTarget, Queue<Vector2> currentPath)
        {
            if (!maze.astarMove.Paths.Contains(newTarget))
            {
                return;
            }

            if (currentPath.Count > 0)
            {
                var current = currentPath.Peek();
                currentPath.Clear();
                currentPath.Enqueue(current);
            }

            var worldPos = maze.MapToWorld(newTarget);
            worldPos += Vector2.Down * maze.CellSize.y / 2;
            currentPath.Enqueue(worldPos);
        }
    }
}
