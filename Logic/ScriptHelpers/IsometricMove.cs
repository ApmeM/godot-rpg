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

			var motion = new Vector2(current.x - currentPosition.x, current.y - currentPosition.y) / delta;
			if (motion.Length() > motionSpeed)
			{
				motion = motion.Normalized() * motionSpeed;
			}

			return motion * delta;
		}

		public static void MoveBy(Queue<Vector2> currentPath, Vector2 currentPosition, Vector2 newTarget, Maze maze)
        {
			var playerPosition = maze.WorldToMap(currentPosition);
			var newPath = AStarPathfinder.Search(maze.astar, new Point((int)playerPosition.x, (int)playerPosition.y), new Point((int)newTarget.x, (int)newTarget.y));
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
				var worldPos = maze.MapToWorld(new Vector2(newPath[i].X, newPath[i].Y));
				worldPos += Vector2.Down * maze.CellSize.y / 2;
				currentPath.Enqueue(worldPos);
			}
		}
	}
}
