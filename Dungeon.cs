using BrainAI.Pathfinding;
using BrainAI.Pathfinding.AStar;
using Godot;
using IsometricGame.Controllers;
using IsometricGame.Logic;
using IsometricGame.Logic.Models;
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
		var maze = GetNode<Maze>("Maze");

		server.Start();
		var firstPlayer = server.Connect("First");
		this.PlayerIdx = firstPlayer;

		var initialData = server.GetInitialData(firstPlayer);
		astar = new AstarGridGraph(initialData.MapWidth, initialData.MapHeight);

		var trollScene = ResourceLoader.Load<PackedScene>("Troll.tscn");

		foreach(var unit in initialData.Units)
		{
			var troll = (Troll)trollScene.Instance();
			maze.AddChild(troll);
			troll.PlayerIdx = firstPlayer;
			troll.UnitIdx = unit.UnitId;
			troll.Position = maze.MapToWorld(new Vector2(unit.PositionX, unit.PositionY));
			troll.Position += Vector2.Down * maze.CellSize.y / 2;

			GetNode<Camera2D>("DraggableCamera").Position = troll.Position;
		}
	}

	public override void _Process(float delta)
	{
		base._Process(delta);

		var maze = GetNode<Maze>("Maze");
		var newTarget = controllerTypes[Controller].GetNewTarget(maze);
		if (newTarget != Vector2.Zero)
		{
			server.PlayerMove(this.PlayerIdx, 1, newTarget);
		}

		var visibleMap = server.GetVisibleMap(this.PlayerIdx);

		maze.NewVisibleMap(visibleMap);

		for (var x = 0; x < visibleMap.GetLength(0); x++)
			for (var y = 0; y < visibleMap.GetLength(1); y++)
			{
				switch (visibleMap[x, y])
				{
					case MapTile.Wall:
						{
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
