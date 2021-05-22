using Godot;
using IsometricGame.Controllers;
using IsometricGame.Logic;
using System.Collections.Generic;
using System.Linq;

public class Dungeon : Node2D
{
	public static Server server = new Server();
	private int PlayerIdx;
	
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
		maze.Initialize(initialData.MapWidth, initialData.MapHeight);

		var trollScene = ResourceLoader.Load<PackedScene>("Troll.tscn");

		foreach(var unit in initialData.Units)
		{
			var troll = (Troll)trollScene.Instance();
			maze.AddChild(troll);
			troll.Unit = unit;
			troll.Position = maze.MapToWorld(new Vector2(unit.PositionX, unit.PositionY));
			troll.Position += Vector2.Down * maze.CellSize.y / 2;
			troll.IsSelected = unit.UnitId == 1;
			troll.AddToGroup("ControllableUnit");

			GetNode<Camera2D>("DraggableCamera").Position = troll.Position;
		}
	}

	public override void _Process(float delta)
	{
		base._Process(delta);

		var maze = GetNode<Maze>("Maze");
		var newTarget = controllerTypes[Controller].GetNewTarget(maze);

		var trolls = this.GetTree().GetNodesInGroup("ControllableUnit").Cast<Troll>().ToList();

		if (newTarget != Vector2.Zero)
		{
			var clickOnTroll = trolls.FirstOrDefault(a => maze.WorldToMap(a.Position) == newTarget);
			var currentTroll = trolls.First(a => a.IsSelected);

			if (clickOnTroll != null)
			{
				currentTroll.IsSelected = false;
				clickOnTroll.IsSelected = true;
			}
			else
			{
				currentTroll.MoveShadowTo(newTarget);
			}
		}

		if (trolls.All(a => a.NewTarget != Vector2.Zero))
		{
			foreach(var troll in trolls)
			{
				server.PlayerMove(troll.Unit.PlayerId, troll.Unit.UnitId, troll.NewTarget);
			}
		}

		var visibleMap = server.GetVisibleMap(this.PlayerIdx);
		maze.NewVisibleMap(visibleMap);
	}
}
