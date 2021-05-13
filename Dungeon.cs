using BrainAI.Pathfinding;
using BrainAI.Pathfinding.AStar;
using Godot;
using IsometricGame.Controllers;
using IsometricGame.Logic;
using System.Collections.Generic;

public class Dungeon : Node2D
{
	public static Server server = new Server();
	private int PlayerIdx;
	public static AstarGridGraph astar;

	public enum ControllerType
	{
		Mouse,
		AI,
		Network
	}

	public Dictionary<ControllerType, IController> controllerTypes = new Dictionary<ControllerType, IController>
	{
		{ControllerType.Mouse, new MouseController() },
		{ControllerType.AI, new AIController() },
	};

	[Export]
	public ControllerType Controller;

	public override void _Ready()
	{
		var tileMapWalls = GetNode<TileMap>("Walls");
		var tileMapFloor = GetNode<TileMap>("Floor");
		tileMapWalls.Clear();
		tileMapFloor.Clear();

		server.Start();
		var firstPlayer = server.Connect("First");

		var initialData = server.GetInitialData(firstPlayer);
		astar = new AstarGridGraph(initialData.MapWidth, initialData.MapHeight);

		var troll = GetNode<Troll>("Walls/Troll");
		troll.PlayerIdx = firstPlayer;
		this.PlayerIdx = firstPlayer;
		troll.Position = tileMapWalls.MapToWorld(new Vector2(initialData.PositionX, initialData.PositionY));
		troll.Position += Vector2.Down * tileMapWalls.CellSize.y / 2;
	}

	public override void _Process(float delta)
	{
		base._Process(delta);

		var tileMapWalls = GetNode<TileMap>("Walls");
		var tileMapFloor = GetNode<TileMap>("Floor");
		
		var newTarget = controllerTypes[Controller].GetNewTarget(tileMapWalls);
		if (newTarget != Vector2.Zero)
		{
			server.PlayerMove(this.PlayerIdx, newTarget);
		}

		var maze = server.GetVisibleMap(this.PlayerIdx);

		for (var x = 0; x < maze.GetLength(0); x++)
			for (var y = 0; y < maze.GetLength(1); y++)
			{
				switch (maze[x, y])
				{
					case MapTile.Path:
						{
							tileMapFloor.SetCellv(new Vector2(x, y), 0);
							break;
						}
					case MapTile.Wall:
						{
							tileMapWalls.SetCellv(new Vector2(x, y), 2);
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
}
