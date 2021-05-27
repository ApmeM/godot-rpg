using BrainAI.Pathfinding.BreadthFirst;
using FateRandom;
using Godot;
using IsometricGame.Logic.Models;
using IsometricGame.Logic.ScriptHelpers;
using System;
using System.Collections.Generic;

public class Maze : TileMap
{
	public VectorGridGraph astar;

	[Signal]
	public delegate void CellSelected(Vector2 cell, Vector2 cellPosition, bool moveAvailable);

	public override void _Ready()
	{
		var floor = GetNode<TileMap>("Floor");
		this.Clear();
		floor.Clear();
	}

	public void NewVisibleMap(MapTile[,] maze)
	{
		var floor = GetNode<TileMap>("Floor");
		
		for (var x = 0; x < maze.GetLength(0); x++)
			for (var y = 0; y < maze.GetLength(1); y++)
			{
				switch (maze[x, y])
				{
					case MapTile.Path:
						{
							var pos = new Vector2(x, y);
							if (floor.GetCellv(pos) == -1)
							{
								floor.SetCellv(pos, Fate.GlobalFate.Chance(90) ? 1 : 0);
							}
							break;
						}
					case MapTile.Wall:
						{
							var position = new Vector2(x, y);
							this.SetCellv(position, 2);
							astar.Walls.Add(position);
							break;
						}
					default:
						{
							break;
						}
				}
			}
	}

	public override void _Process(float delta)
	{
		base._Process(delta);
		
		var floor = GetNode<TileMap>("Floor");

		var mouse = floor.GetGlobalMousePosition();
		var cell = floor.WorldToMap(mouse);

		var cellHighliter = floor.GetNode<Line2D>("CellHighlighter");
		if (floor.GetCellv(cell) == 0 || floor.GetCellv(cell) == 1 || floor.GetCellv(cell) == 5)
		{
			cellHighliter.Position = floor.MapToWorld(cell);
			cellHighliter.Visible = true;
		}
		else
		{
			cellHighliter.Visible = false;
		}
	}

	public void Initialize(int mapWidth, int mapHeight)
	{
		astar = new VectorGridGraph(mapWidth, mapHeight);
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		base._Input(@event);

		if (@event is InputEventScreenTouch eventMouseButton && eventMouseButton.Pressed)
		{
			var position = this.GetGlobalMousePosition(); // eventMouseButton.Position;
			var cell = this.WorldToMap(position);
			
			var floor = GetNode<TileMap>("Floor");
			if (floor.GetCellv(cell) == 0 || floor.GetCellv(cell) == 1 || floor.GetCellv(cell) == 5)
			{
				var cellPosition = this.MapToWorld(cell);
				EmitSignal(nameof(CellSelected), cell, cellPosition, floor.GetCellv(cell) == 5);
				GetTree().SetInputAsHandled();
			}
		}
	}

	private readonly List<Vector2> highlitedCells = new List<Vector2>();
	private readonly List<Vector2> directions = new List<Vector2> {
		Vector2.Up,
		Vector2.Down,
		Vector2.Left,
		Vector2.Right
	};

	public void HighliteAvailableMoves(Vector2 unitCell, int? moveDistance)
	{
		if(moveDistance == null)
		{
			throw new Exception("Unknown move distance to highlite. Possible reason - trying to highlite distance for enemy unit.");
		}

		var floor = GetNode<TileMap>("Floor");
		foreach (var cell in highlitedCells)
		{
			floor.SetCellv(cell, Fate.GlobalFate.Chance(90) ? 1 : 0);
		}

		BreadthFirstPathfinder.Search(this.astar, unitCell, moveDistance.Value, out var visited);

		foreach(var cell in visited.Keys)
		{
			floor.SetCellv(cell, 5);
			highlitedCells.Add(cell);
		}
	}
}
