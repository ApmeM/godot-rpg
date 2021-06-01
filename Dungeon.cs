using Godot;
using IsometricGame.Logic;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using System.Linq;

public class Dungeon : Node2D
{
	public static Server server = new Server();
	private int PlayerId;
	private CurrentAction currentAction = CurrentAction.None;

	public override void _Ready()
	{
		var maze = GetNode<Maze>("Maze");

		server.Start();
		this.PlayerId = server.Connect("First");

		var initialData = server.GetInitialData(this.PlayerId);
		maze.Initialize(initialData.VisibleMap.GetLength(0), initialData.VisibleMap.GetLength(1));
		maze.NewVisibleMap(initialData.VisibleMap);
		maze.Connect(nameof(Maze.CellSelected), this, nameof(CellSelected));

		var unitScene = ResourceLoader.Load<PackedScene>("Unit.tscn");

		foreach (var unit in initialData.YourUnits)
		{
			var unitSceneInstance = (Unit)unitScene.Instance();
			maze.AddChild(unitSceneInstance);
			unitSceneInstance.ClientUnit = new ClientUnit
			{
				PlayerId = this.PlayerId,
				UnitId = unit.UnitId,
				MoveDistance = unit.MoveDistance,
				SightRange = unit.SightRange
			};
			unitSceneInstance.Position = maze.MapToWorld(unit.Position);
			unitSceneInstance.Position += Vector2.Down * maze.CellSize.y / 2;
			unitSceneInstance.IsSelected = false;
			unitSceneInstance.AddToGroup(Groups.MyUnits);

			GetNode<Camera2D>("DraggableCamera").Position = unitSceneInstance.Position;
		}

		foreach(var player in initialData.OtherPlayers)
		{
			foreach (var unit in player.Units)
			{
				var unitSceneInstance = (Unit)unitScene.Instance();
				maze.AddChild(unitSceneInstance);
				unitSceneInstance.ClientUnit = new ClientUnit
				{
					PlayerId = player.PlayerId,
					UnitId = unit,
					MoveDistance = null
				};
				unitSceneInstance.Visible = false;
				unitSceneInstance.AddToGroup(Groups.OtherUnits);
			}
		}

		GetNode<Button>("CanvasLayer/NextTurnButton").Connect("pressed", this, nameof(NextTurnPressed));
		GetNode<UnitActions>("UnitActions").Connect(nameof(UnitActions.ActionSelected), this, nameof(UnitActionSelected));
	}

	private void UnitActionSelected(CurrentAction action)
	{
		this.currentAction = action;
		GetNode<Control>("UnitActions").Visible = false;

		switch (this.currentAction)
		{
			case CurrentAction.Move:
				{
					var maze = GetNode<Maze>("Maze");
					var units = this.GetTree().GetNodesInGroup(Groups.MyUnits).Cast<Unit>().ToList();
					var currentUnit = units.FirstOrDefault(a => a.IsSelected);
					maze.HighliteAvailableMoves(maze.WorldToMap(currentUnit.Position), currentUnit.ClientUnit.MoveDistance);
					break;
				}
		}
	}

	public void CellSelected(Vector2 cell, Vector2 cellPosition, bool moveAvailable)
	{
		var maze = GetNode<Maze>("Maze");
		var units = this.GetTree().GetNodesInGroup(Groups.MyUnits).Cast<Unit>().ToList();

		var currentUnit = units.FirstOrDefault(a => a.IsSelected);

		switch (this.currentAction)
		{
			case CurrentAction.None:
				{
					var clickOnUnit = units.FirstOrDefault(a => maze.WorldToMap(a.Position) == cell || a.NewTarget == cell);
					if (currentUnit != null)
					{
						currentUnit.IsSelected = false;
					}
					GetNode<UnitDetails>("CanvasLayer/UnitDetails").SelectUnit(clickOnUnit?.ClientUnit);
					var unitActions = GetNode<Control>("UnitActions");
					unitActions.Visible = clickOnUnit != null;

					if (clickOnUnit != null)
					{
						clickOnUnit.IsSelected = true;
						unitActions.RectPosition = clickOnUnit.Position;
					}
					break;
				}
			case CurrentAction.Move:
				{
					if (moveAvailable)
					{
						this.currentAction = CurrentAction.None;
						currentUnit.MoveShadowTo(cell);
						var unitActions = GetNode<Control>("UnitActions");
						unitActions.RectPosition = cellPosition;
						unitActions.Visible = true;
						maze.RemoveHighliting();
					}
					break;
				}
			case CurrentAction.Attack:
				{
					this.currentAction = CurrentAction.None;
					break;
				}
		}
	}

	public void NextTurnPressed()
	{
		var maze = GetNode<Maze>("Maze");
		var myUnits = this.GetTree().GetNodesInGroup(Groups.MyUnits).Cast<Unit>().ToList();

		server.PlayerMove(this.PlayerId, myUnits.ToDictionary(a => a.ClientUnit.UnitId, a => a.NewTarget));

		var turnData = server.GetTurnData(this.PlayerId);

		maze.NewVisibleMap(turnData.VisibleMap);

		foreach (var unit in myUnits)
		{
			unit.MoveUnitTo(turnData.YourUnits[unit.ClientUnit.UnitId].Position);
			if (unit.IsSelected)
			{
				maze.HighliteAvailableMoves(turnData.YourUnits[unit.ClientUnit.UnitId].Position, unit.ClientUnit.MoveDistance);
			}
		}

		var otherUnits = this.GetTree().GetNodesInGroup(Groups.OtherUnits).Cast<Unit>().ToList();
		foreach (var unit in otherUnits)
		{
			var player = turnData.OtherPlayers[unit.ClientUnit.PlayerId];
			if (!player.Units.ContainsKey(unit.ClientUnit.UnitId))
			{
				unit.Visible = false;
				continue;
			}

			if (unit.Visible)
			{
				unit.MoveUnitTo(player.Units[unit.ClientUnit.UnitId].Position);
			}
			else
			{
				unit.Position = maze.MapToWorld(player.Units[unit.ClientUnit.UnitId].Position);
				unit.Visible = true;
			}
		}
	}
}
