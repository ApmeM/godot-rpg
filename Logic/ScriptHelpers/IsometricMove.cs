using BrainAI.Pathfinding;
using BrainAI.Pathfinding.AStar;
using Godot;
using System;
using System.Collections.Generic;

namespace IsometricGame.Logic.ScriptHelpers
{
    public static class IsometricMove
    {
        public static string Animate(Vector2 motion, string left = "left", string right = "right", string up = "up", string down = "down")
        {
			if (motion.x > 0)
			{
				if (motion.y > 0)
				{
					return right;
				}
				else if (motion.y < 0)
				{
					return up;
				}
			}
			else if (motion.x < 0)
			{
				if (motion.y > 0)
				{
					return down;
				}
				else if (motion.y < 0)
				{
					return left;
				}
			}

			return null;
		}

		public static Vector2? GetMotion(Queue<Vector2> currentPath, Vector2 currentPosition, float delta, int motionSpeed)
        {
			if (currentPath.Count == 0)
			{
				return null;
			}

			var current = currentPath.Peek();
			if (Math.Abs(current.x - currentPosition.x) < 1 && Math.Abs(current.y - currentPosition.y) < 1)
			{
				currentPath.Dequeue();
				return null;
			}

			var motion = (current - currentPosition) / delta;
			if (motion.Length() > motionSpeed)
			{
				motion = motion.Normalized() * motionSpeed;
			}

			return motion * delta;
		}

		public static void MoveBy(Queue<Vector2> currentPath, Vector2 currentPosition, Vector2 newTarget, Maze maze)
        {
			var playerPosition = maze.WorldToMap(currentPosition);
			var newPath = AStarPathfinder.Search(maze.astar, playerPosition, newTarget);
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
				var worldPos = maze.MapToWorld(newPath[i]);
				worldPos += Vector2.Down * maze.CellSize.y / 2;
				currentPath.Enqueue(worldPos);
			}
		}
	}
}
