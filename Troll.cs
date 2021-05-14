using BrainAI.Pathfinding;
using BrainAI.Pathfinding.AStar;
using Godot;
using System;
using System.Collections.Generic;

public class Troll : KinematicBody2D
{
	const int MOTION_SPEED = 800;
	private readonly Queue<Point> path = new Queue<Point>();
	private Vector2 oldTarget;

	public int PlayerIdx { get; set; }

	public override void _PhysicsProcess(float delta)
	{
		base._PhysicsProcess(delta);
		if (path.Count == 0)
		{
			return;
		}
		
		var current = path.Peek();
		if (Math.Abs(current.X - Position.x) < 1 && Math.Abs(current.Y - Position.y) < 1)
		{
			path.Dequeue();
			return;
		}

		var motion = new Vector2(current.X - Position.x, current.Y - Position.y) / delta;
		if (motion.Length() > MOTION_SPEED)
		{
			motion = motion.Normalized() * MOTION_SPEED;
		}
		
		MoveAndSlide(motion);
	}

	public void MoveBy(List<Point> path)
	{
		if (this.path.Count > 0)
		{
			var current = this.path.Peek();
			this.path.Clear();
			this.path.Enqueue(current);
		}

		var tileMapWalls = GetParent<TileMap>();

		for (var i = 0; i < path.Count; i++)
		{
			var worldPos = tileMapWalls.MapToWorld(new Vector2(path[i].X, path[i].Y));
			worldPos += Vector2.Down * tileMapWalls.CellSize.y / 2;
			this.path.Enqueue(new Point((int)worldPos.x, (int)worldPos.y));
		}
	}

	public override void _Process(float delta)
	{
		base._Process(delta);

		var tileMapWalls = GetParent<TileMap>();

		var player = Dungeon.server.GetPlayer(this.PlayerIdx);

		var newTarget = new Vector2(player.PositionX, player.PositionY);
		if (newTarget != this.oldTarget)
		{
			this.oldTarget = newTarget;
			var playerPosition = tileMapWalls.WorldToMap(Position);
			var path = AStarPathfinder.Search(Dungeon.astar, new Point((int)playerPosition.x, (int)playerPosition.y), new Point((int)newTarget.x, (int)newTarget.y));
			if (path != null)
			{
				MoveBy(path);
			}
		}
		
		var animation = GetNode<AnimatedSprite>("AnimatedSprite");
		animation.Playing = this.path.Count != 0;
		if (this.path.Count != 0)
		{
			var current = this.path.Peek();
			if (current.X > Position.x)
			{
				if (current.Y > Position.y)
				{
					animation.Animation = "right";
				}
				else if (current.Y < Position.y)
				{
					animation.Animation = "up";
				}
			}
			else if (current.X < Position.x)
			{
				if (current.Y > Position.y)
				{
					animation.Animation = "down";
				}
				else if (current.Y < Position.y)
				{
					animation.Animation = "left";
				}
			}
		}
	}
}
