using Godot;
using IsometricGame.Logic;
using System.Linq;

public class Dungeon : Node2D
{
	public static Server server = new Server();
	private int PlayerId;
	
	public override void _Ready()
	{
		var maze = GetNode<Maze>("Maze");

		server.Start();
		var firstPlayer = server.Connect("First");
		this.PlayerId = firstPlayer;

		var initialData = server.GetInitialData(firstPlayer);
		maze.Initialize(initialData.MapWidth, initialData.MapHeight);
		maze.NewVisibleMap(initialData.VisibleMap);
		maze.Connect(nameof(Maze.CellSelected), this, nameof(CellSelected));

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

		GetNode<Button>("CanvasLayer/NextTurnButton").Connect("pressed", this, nameof(NextTurnPressed));
	}

	public void CellSelected(Vector2 cell, Vector2 cellPosition)
	{
		var maze = GetNode<Maze>("Maze");
		var trolls = this.GetTree().GetNodesInGroup("ControllableUnit").Cast<Troll>().ToList();

		var clickOnTroll = trolls.FirstOrDefault(a => maze.WorldToMap(a.Position) == cell);
		var currentTroll = trolls.First(a => a.IsSelected);

		if (clickOnTroll != null)
		{
			currentTroll.IsSelected = false;
			clickOnTroll.IsSelected = true;
		}
		else
		{
			currentTroll.MoveShadowTo(cell);
		}
	}

	public void NextTurnPressed()
	{
		var maze = GetNode<Maze>("Maze");
		var trolls = this.GetTree().GetNodesInGroup("ControllableUnit").Cast<Troll>().ToList();

		foreach (var troll in trolls)
		{
			server.PlayerMove(troll.Unit.PlayerId, troll.Unit.UnitId, troll.NewTarget);
		}

		var player = server.GetPlayer(this.PlayerId);
		foreach (var troll in trolls)
		{
			var moveTarget = new Vector2(player.Units[troll.Unit.UnitId].PositionX, player.Units[troll.Unit.UnitId].PositionY);
			troll.MoveUnitTo(moveTarget);
		}

		var visibleMap = server.GetVisibleMap(this.PlayerId);
		maze.NewVisibleMap(visibleMap);
	}
}
