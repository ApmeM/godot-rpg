using Godot;
using IsometricGame.Logic;
using IsometricGame.Logic.Models;
using System.Linq;

public class Dungeon : Node2D
{
	public static Server server = new Server();
	private int PlayerId;
	
	public override void _Ready()
	{
		var maze = GetNode<Maze>("Maze");

		server.Start();
		this.PlayerId = server.Connect("First");

		var initialData = server.GetInitialData(this.PlayerId);
		maze.Initialize(initialData.VisibleMap.GetLength(0), initialData.VisibleMap.GetLength(1));
		maze.NewVisibleMap(initialData.VisibleMap);
		maze.Connect(nameof(Maze.CellSelected), this, nameof(CellSelected));

		var trollScene = ResourceLoader.Load<PackedScene>("Troll.tscn");

		foreach (var unit in initialData.YourUnits)
		{
			var troll = (Troll)trollScene.Instance();
			maze.AddChild(troll);
			troll.Unit = new ClientUnit
			{
				PlayerId = this.PlayerId,
				UnitId = unit.UnitId,
				MoveDistance = unit.MoveDistance
			};
			troll.Position = maze.MapToWorld(unit.Position);
			troll.Position += Vector2.Down * maze.CellSize.y / 2;
			troll.IsSelected = false;
			troll.AddToGroup(Groups.MyUnits);

			GetNode<Camera2D>("DraggableCamera").Position = troll.Position;
		}

		foreach(var player in initialData.OtherPlayers)
		{
			foreach (var unit in player.Units)
			{
				var troll = (Troll)trollScene.Instance();
				maze.AddChild(troll);
				troll.Unit = new ClientUnit
				{
					PlayerId = player.PlayerId,
					UnitId = unit,
					MoveDistance = null
				};
				troll.Visible = false;
				troll.AddToGroup(Groups.OtherUnits);
			}
		}

		GetNode<Button>("CanvasLayer/NextTurnButton").Connect("pressed", this, nameof(NextTurnPressed));
	}

	public void CellSelected(Vector2 cell, Vector2 cellPosition, bool moveAvailable)
	{
		var maze = GetNode<Maze>("Maze");
		var trolls = this.GetTree().GetNodesInGroup(Groups.MyUnits).Cast<Troll>().ToList();

		var clickOnTroll = trolls.FirstOrDefault(a => maze.WorldToMap(a.Position) == cell);
		var currentTroll = trolls.FirstOrDefault(a => a.IsSelected);

		if (clickOnTroll != null)
		{
			if (currentTroll != null)
			{
				currentTroll.IsSelected = false;
			}

			clickOnTroll.IsSelected = true;
			maze.HighliteAvailableMoves(cell, clickOnTroll.Unit.MoveDistance);
		}
		else if(moveAvailable)
		{
			currentTroll?.MoveShadowTo(cell);
		}
	}

	public void NextTurnPressed()
	{
		var maze = GetNode<Maze>("Maze");
		var myUnits = this.GetTree().GetNodesInGroup(Groups.MyUnits).Cast<Troll>().ToList();

		server.PlayerMove(this.PlayerId, myUnits.ToDictionary(a => a.Unit.UnitId, a => a.NewTarget));

		var turnData = server.GetTurnData(this.PlayerId);

		maze.NewVisibleMap(turnData.VisibleMap);

		foreach (var unit in myUnits)
		{
			unit.MoveUnitTo(turnData.YourUnits[unit.Unit.UnitId].Position);
			if (unit.IsSelected)
			{
				maze.HighliteAvailableMoves(turnData.YourUnits[unit.Unit.UnitId].Position, unit.Unit.MoveDistance);
			}
		}

		var otherUnits = this.GetTree().GetNodesInGroup(Groups.OtherUnits).Cast<Troll>().ToList();
		foreach (var unit in otherUnits)
		{
			var player = turnData.OtherPlayers[unit.Unit.PlayerId];
			if (!player.Units.ContainsKey(unit.Unit.UnitId))
			{
				unit.Visible = false;
				continue;
			}

			if (unit.Visible)
			{
				unit.MoveUnitTo(player.Units[unit.Unit.UnitId].Position);
			}
			else
			{
				unit.Position = maze.MapToWorld(player.Units[unit.Unit.UnitId].Position);
				unit.Visible = true;
			}
		}
	}
}
