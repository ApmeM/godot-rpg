using BrainAI.Pathfinding.BreadthFirst;
using FateRandom;
using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.ScriptHelpers;
using System;
using System.Collections.Generic;
using System.Linq;

public class Maze : TileMap
{
	private enum HighliteType
	{
		Move = 7,
		AttackRadius = 6,
		AttackDistance = 5,
		HighlitedMove = 8
	}

	private readonly List<Vector2> lastHighlitedCells = new List<Vector2>();
	private readonly Dictionary<HighliteType, List<Vector2>> highlitedCells;

	public VectorGridGraph astar;
	private int? attackRadius;

	[Signal]
	public delegate void CellSelected(Vector2 cell, Vector2 cellPosition, bool moveAvailable);

	public Maze()
	{
		this.highlitedCells = Enum.GetValues(typeof(HighliteType)).Cast<HighliteType>().ToDictionary(a => a, a => new List<Vector2>());
	}

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

		if (floor.GetCellv(cell) > 0)
		{
			highlitedCells[HighliteType.HighlitedMove].Clear();
			highlitedCells[HighliteType.HighlitedMove].Add(cell);
		}

		if (attackRadius != null && highlitedCells[HighliteType.AttackDistance].Contains(cell))
		{
			HighliteRadius(cell, attackRadius.Value, HighliteType.AttackRadius);
		}
		else
		{
			highlitedCells[HighliteType.AttackRadius].Clear();
		}

		RehighliteCells();
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
			if (floor.GetCellv(cell) >= 0)
			{
				var cellPosition = this.MapToWorld(cell);
				EmitSignal(nameof(CellSelected), cell, cellPosition, highlitedCells[HighliteType.Move].Contains(cell) || highlitedCells[HighliteType.AttackDistance].Contains(cell));
				GetTree().SetInputAsHandled();
			}
		}
	}

	private void RehighliteCells()
	{
		var floor = GetNode<TileMap>("Floor");
		foreach (var cell in lastHighlitedCells)
		{
			floor.SetCellv(cell, Fate.GlobalFate.Chance(90) ? 1 : 0);
		}

		foreach(var highlitedCell in highlitedCells)
		{
			foreach (var cell in highlitedCell.Value)
			{
				floor.SetCellv(cell, (int)highlitedCell.Key);
			}
			lastHighlitedCells.AddRange(highlitedCell.Value);

		}
	}

	public void RemoveHighliting()
	{
		foreach(var cells in highlitedCells.Values)
		{
			cells.Clear();
		}
		RehighliteCells();
	}

	private void HighliteRadius(Vector2 fromPoint, int highliteRadius, HighliteType highliteType)
	{
		highlitedCells[highliteType].Clear();

		BreadthFirstPathfinder.Search(this.astar, fromPoint, highliteRadius, out var visited);

		foreach (var cell in visited.Keys)
		{
			highlitedCells[highliteType].Add(cell);
		}
	}

	public void HighliteAvailableAttacks(Vector2 shadowCell, int? attackDistance, int? attackRadius)
	{
		this.attackRadius = attackRadius;
		HighliteRadius(shadowCell, attackDistance.Value, HighliteType.AttackDistance);
		RehighliteCells();
	}

	public void HighliteAvailableMoves(Vector2 unitCell, int? moveDistance)
	{
		if (moveDistance == null)
		{
			throw new Exception("Unknown move distance to highlite. Possible reason - trying to highlite distance for enemy unit.");
		}
		
		this.attackRadius = null;
		HighliteRadius(unitCell, moveDistance.Value, HighliteType.Move);
		RehighliteCells();
	}
}
