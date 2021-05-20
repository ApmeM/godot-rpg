using BrainAI.Pathfinding;
using BrainAI.Pathfinding.AStar;
using Godot;
using IsometricGame.Logic.Models;
using System;
using System.Collections.Generic;

public class Troll : Node2D
{
	private const int MOTION_SPEED = 800;
	private readonly Queue<Point> path = new Queue<Point>();
	private readonly Queue<Point> shadowPath = new Queue<Point>();
	private Vector2 oldTarget;
	
	public Unit Unit;
	public bool IsSelected;
	public Vector2 NewTarget = Vector2.Zero;

	public void ProcessMove(float delta, bool isShadow)
	{
		var animation = isShadow ? GetNode<AnimatedSprite>("Shadow") : GetNode<AnimatedSprite>("AnimatedSprite");
		var currentPath = isShadow ? this.shadowPath : this.path;
		var position = isShadow ? animation.Position : Position;
		animation.Playing = currentPath.Count != 0;

		if (currentPath.Count == 0)
		{
			return;
		}

		var current = currentPath.Peek();
		if (Math.Abs(current.X - position.x) < 1 && Math.Abs(current.Y - position.y) < 1)
		{
			currentPath.Dequeue();
			return;
		}

		var motion = new Vector2(current.X - position.x, current.Y - position.y) / delta;
		if (motion.Length() > MOTION_SPEED)
		{
			motion = motion.Normalized() * MOTION_SPEED;
		}

		position += motion * delta;
		if (isShadow)
		{
			animation.Position = position;
		}
		else
		{
			Position = position;
		}

		if (motion.x > 0)
		{
			if (motion.y > 0)
			{
				animation.Animation = "right";
			}
			else if (motion.y < 0)
			{
				animation.Animation = "up";
			}
		}
		else if (motion.x < 0)
		{
			if (motion.y > 0)
			{
				animation.Animation = "down";
			}
			else if (motion.y < 0)
			{
				animation.Animation = "left";
			}
		}
	}

	public void MoveBy(List<Point> path, bool isShadow)
	{
		var currentPath = isShadow ? this.shadowPath : this.path;

		if (currentPath.Count > 0)
		{
			var current = currentPath.Peek();
			currentPath.Clear();
			currentPath.Enqueue(current);
		}

		var tileMapWalls = GetParent<TileMap>();

		for (var i = 0; i < path.Count; i++)
		{
			var worldPos = tileMapWalls.MapToWorld(new Vector2(path[i].X, path[i].Y)) - (isShadow ? Position : Vector2.Zero);
			worldPos += Vector2.Down * tileMapWalls.CellSize.y / 2;
			currentPath.Enqueue(new Point((int)worldPos.x, (int)worldPos.y));
		}
	}

	public override void _Process(float delta)
	{
		base._Process(delta);

		GetNode<AnimatedSprite>("Shadow/SelectionMarker").Visible = IsSelected;
		GetNode<AnimatedSprite>("AnimatedSprite/SelectionMarker").Visible = IsSelected;
		//((ShaderMaterial)this.Material).SetShaderParam("grayscale", !IsSelected);

		var maze = GetParent<Maze>();

		var player = Dungeon.server.GetPlayer(this.Unit.PlayerId);

		var newTarget = new Vector2(player.Units[this.Unit.UnitId].PositionX, player.Units[this.Unit.UnitId].PositionY);
		if (newTarget != this.oldTarget)
		{
			this.oldTarget = newTarget;
			this.NewTarget = Vector2.Zero;
			this.shadowPath.Clear();
			GetNode<AnimatedSprite>("Shadow").Position = new Vector2(0, -25);
			var playerPosition = maze.WorldToMap(Position);
			var path = AStarPathfinder.Search(maze.astar, new Point((int)playerPosition.x, (int)playerPosition.y), new Point((int)newTarget.x, (int)newTarget.y));
			if (path != null)
			{
				MoveBy(path, false);
			}
		}

		ProcessMove(delta, false);
		ProcessMove(delta, true);
	}

	public void MoveShadowTo(Vector2 newTarget)
	{
		var maze = GetParent<Maze>();

		var playerPosition = maze.WorldToMap(Position + GetNode<AnimatedSprite>("Shadow").Position);
		var path = AStarPathfinder.Search(maze.astar, new Point((int)playerPosition.x, (int)playerPosition.y), new Point((int)newTarget.x, (int)newTarget.y));
		if (path != null)
		{
			MoveBy(path, true);
		}

		this.NewTarget = newTarget;
	}
}
