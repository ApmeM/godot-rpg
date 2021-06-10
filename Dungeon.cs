using Godot;
using IsometricGame.Logic;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using System.Collections.Generic;
using System.Linq;

public class Dungeon : Node2D
{
	public static Server server = new Server();
	private int PlayerId;
	private CurrentAction currentAction = CurrentAction.None;

	public override void _Ready()
	{
		var maze = GetNode<Maze>("Maze");

		server.Start(new ServerConfiguration
		{
			FullMapVisible = true
		});
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
				SightRange = unit.SightRange,
				AttackDistance = unit.AttackDistance,
				AttackRadius = unit.AttackRadius
			};
			unitSceneInstance.Position = maze.MapToWorld(unit.Position);
			unitSceneInstance.Position += Vector2.Down * maze.CellSize.y / 2;
			unitSceneInstance.IsSelected = false;
			unitSceneInstance.AddToGroup(Groups.MyUnits);

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
				};
				unitSceneInstance.Visible = false;
				unitSceneInstance.AddToGroup(Groups.OtherUnits);
			}
		}

		GetNode<Camera2D>("DraggableCamera").Position = maze.MapToWorld(initialData.YourUnits[0].Position) + Vector2.Down * maze.CellSize.y / 2;
		GetNode<Button>("CanvasLayer/NextTurnButton").Connect("pressed", this, nameof(NextTurnPressed));
		GetNode<UnitActions>("UnitActions").Connect(nameof(UnitActions.ActionSelected), this, nameof(UnitActionSelected));
	}

	private void UnitActionSelected(CurrentAction action)
	{
		this.currentAction = action;
		GetNode<Control>("UnitActions").Visible = false;

		var maze = GetNode<Maze>("Maze");
		var units = this.GetTree().GetNodesInGroup(Groups.MyUnits).Cast<Unit>().ToList();
		var currentUnit = units.FirstOrDefault(a => a.IsSelected);

		switch (this.currentAction)
		{
			case CurrentAction.Move:
				{
					maze.HighliteAvailableMoves(maze.WorldToMap(currentUnit.Position), currentUnit.ClientUnit.MoveDistance);
					break;
				}
			case CurrentAction.Attack:
				{
					maze.HighliteAvailableAttacks(currentUnit.NewTarget == null ? maze.WorldToMap(currentUnit.Position) : currentUnit.NewTarget.Value, currentUnit.ClientUnit.AttackDistance, currentUnit.ClientUnit.AttackRadius);
					break;
				}
		}
	}

	public void CellSelected(Vector2 cell, Vector2 cellPosition, bool moveAvailable)
	{
		var maze = GetNode<Maze>("Maze");
		var units = this.GetTree().GetNodesInGroup(Groups.MyUnits).Cast<Unit>().ToList();

		var currentUnit = units.FirstOrDefault(a => a.IsSelected);
		var unitActions = GetNode<Control>("UnitActions");

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
					unitActions.Visible = clickOnUnit != null && !clickOnUnit.IsDead;
					unitActions.RectPosition = this.GetViewport().CanvasTransform.AffineInverse().Xform(GetViewport().GetMousePosition());

					if (clickOnUnit != null)
					{
						clickOnUnit.IsSelected = true;
					}
					break;
				}
			case CurrentAction.Move:
				{
					if (moveAvailable)
					{
						this.currentAction = CurrentAction.None;
						currentUnit.MoveShadowTo(cell);
						unitActions.RectPosition = this.GetViewport().CanvasTransform.AffineInverse().Xform(GetViewport().GetMousePosition());
						unitActions.Visible = true;
						maze.RemoveHighliting();
					}
					break;
				}
			case CurrentAction.Attack:
				{
					if (moveAvailable)
					{
						this.currentAction = CurrentAction.None;
						currentUnit.AttackShadowTo(cell);
						unitActions.RectPosition = this.GetViewport().CanvasTransform.AffineInverse().Xform(GetViewport().GetMousePosition());
						unitActions.Visible = true;
						maze.RemoveHighliting();
					}
					break;
				}
		}
	}

	public void NextTurnPressed()
	{
		var myUnits = this.GetTree().GetNodesInGroup(Groups.MyUnits).Cast<Unit>().ToList();
		foreach (var unit in myUnits)
		{
			unit.IsSelected = false;
		}
		GetNode<Control>("UnitActions").Visible = false;
		currentAction = CurrentAction.None;
		var maze = GetNode<Maze>("Maze");
		maze.RemoveHighliting();
		GetNode<Button>("CanvasLayer/NextTurnButton").Visible = false;

		server.PlayerMove(this.PlayerId, new TransferTurnDoneData
		{
			UnitActions = myUnits.ToDictionary(a => a.ClientUnit.UnitId, a => new TransferTurnDoneUnit
			{
				Move = a.NewTarget,
				Attack = a.AttackDirection
			})
		});
		var turnData = server.GetTurnData(this.PlayerId);

		maze.NewVisibleMap(turnData.VisibleMap);
		TurnDone(turnData);
	}

	private async void TurnDone(TransferTurnData turnData)
	{
		var maze = GetNode<Maze>("Maze");
		var myUnits = this.GetTree().GetNodesInGroup(Groups.MyUnits).Cast<Unit>().ToList();
		var otherUnits = this.GetTree().GetNodesInGroup(Groups.OtherUnits).Cast<Unit>().ToList();
		var signals = new List<SignalAwaiter>();

		foreach (var unit in myUnits)
		{
			unit.MoveUnitTo(turnData.YourUnits[unit.ClientUnit.UnitId].Position);
			signals.Add(ToSignal(unit, nameof(Unit.MoveDone)));
		}

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
				signals.Add(ToSignal(unit, nameof(Unit.MoveDone)));
			}
			else
			{
				unit.Position = maze.MapToWorld(player.Units[unit.ClientUnit.UnitId].Position);
				unit.Visible = true;
			}
		}

		foreach(var signal in signals)
		{
			await signal;
		}
		signals.Clear();

		foreach (var unit in myUnits)
		{
			var attackDirection = turnData.YourUnits[unit.ClientUnit.UnitId].AttackDirection;
			if (!attackDirection.HasValue)
			{
				continue;
			}
			unit.AnimateUnit("attack", attackDirection.Value);
			signals.Add(ToSignal(unit, nameof(Unit.UnitAnimationDone)));
		}

		foreach (var unit in otherUnits)
		{
			var player = turnData.OtherPlayers[unit.ClientUnit.PlayerId];
			if (!player.Units.ContainsKey(unit.ClientUnit.UnitId))
			{
				continue;
			}

			if (unit.Visible)
			{
				var attackDirection = player.Units[unit.ClientUnit.UnitId].AttackDirection;
				if (!attackDirection.HasValue)
				{
					continue;
				}
				unit.AnimateUnit("attack", attackDirection.Value);
				signals.Add(ToSignal(unit, nameof(Unit.UnitAnimationDone)));
			}
		}

		foreach (var signal in signals)
		{
			await signal;
		}
		signals.Clear();

		foreach (var unit in myUnits)
		{
			var hpReduced = turnData.YourUnits[unit.ClientUnit.UnitId].HpReduced;
			if (!hpReduced.HasValue)
			{
				continue;
			}
			unit.AnimateUnit("hit", turnData.YourUnits[unit.ClientUnit.UnitId].AttackFrom.Value);
			signals.Add(ToSignal(unit, nameof(Unit.UnitAnimationDone)));
		}

		foreach (var unit in otherUnits)
		{
			var player = turnData.OtherPlayers[unit.ClientUnit.PlayerId];
			if (!player.Units.ContainsKey(unit.ClientUnit.UnitId))
			{
				continue;
			}

			if (unit.Visible)
			{
				var hpReduced = player.Units[unit.ClientUnit.UnitId].HpReduced;
				if (!hpReduced.HasValue)
				{
					continue;
				}
				unit.AnimateUnit("hit", player.Units[unit.ClientUnit.UnitId].AttackFrom.Value);
				signals.Add(ToSignal(unit, nameof(Unit.UnitAnimationDone)));
			}
		}

		foreach (var signal in signals)
		{
			await signal;
		}
		signals.Clear();
		foreach (var unit in myUnits)
		{
			var attackFrom = turnData.YourUnits[unit.ClientUnit.UnitId].AttackFrom;
			if (!attackFrom.HasValue || !turnData.YourUnits[unit.ClientUnit.UnitId].IsDead)
			{
				continue;
			}
			unit.AnimateUnit("dead", turnData.YourUnits[unit.ClientUnit.UnitId].AttackFrom.Value);
			signals.Add(ToSignal(unit, nameof(Unit.UnitAnimationDone)));
			unit.IsDead = true;
		}

		foreach (var unit in otherUnits)
		{
			var player = turnData.OtherPlayers[unit.ClientUnit.PlayerId];
			if (!player.Units.ContainsKey(unit.ClientUnit.UnitId))
			{
				continue;
			}

			if (unit.Visible)
			{
				var attackFrom = player.Units[unit.ClientUnit.UnitId].AttackFrom;
				if (!attackFrom.HasValue || !player.Units[unit.ClientUnit.UnitId].IsDead)
				{
					continue;
				}
				unit.AnimateUnit("dead", player.Units[unit.ClientUnit.UnitId].AttackFrom.Value);
				signals.Add(ToSignal(unit, nameof(Unit.UnitAnimationDone)));
				unit.IsDead = true;
			}
		}

		foreach (var signal in signals)
		{
			await signal;
		}
		signals.Clear();

		GetNode<Button>("CanvasLayer/NextTurnButton").Visible = true;
	}
}
