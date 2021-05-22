using BrainAI.Pathfinding;
using BrainAI.Pathfinding.AStar;
using FateRandom;
using Godot;
using IsometricGame.Logic.Models;

public class Maze : TileMap
{
	public AstarGridGraph astar;

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
							this.SetCellv(new Vector2(x, y), 2);
							astar.Walls.Add(new Point(x, y));
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
		if (floor.GetCellv(cell) == 0 || floor.GetCellv(cell) == 1)
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
		astar = new AstarGridGraph(mapWidth, mapHeight);
	}
}
