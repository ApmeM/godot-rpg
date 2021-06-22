using Godot;
using IsometricGame.Logic;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using IsometricGame.Logic.ScriptHelpers;
using System.Collections.Generic;
using System.Linq;

public class Dungeon : Node2D
{
	private int PlayerId;
	private CurrentAction currentAction = CurrentAction.None;
	private IUsable currentUsable = null;
	private Server server;

	public override void _Ready()
	{
		GetNode<Button>("CanvasLayer/NextTurnButton").Connect("pressed", this, nameof(NextTurnPressed));
		GetNode<UnitActions>("UnitActions").Connect(nameof(UnitActions.ActionSelected), this, nameof(UnitActionSelected));
	}

	public void NewGame(Server server)
	{
		this.server = server;
		this.server.Connect(new TransferConnectData
		{
			PlayerName = "Player",
			Units = new List<TransferConnectData.UnitData>
			{
				new TransferConnectData.UnitData{ UnitType = UnitType.Amazon, Skills = new List<Skill>{Skill.VisionRange, Skill.AttackRadius}},
				new TransferConnectData.UnitData{ UnitType = UnitType.Goatman, Skills = new List<Skill>{Skill.VisionRange, Skill.AttackRadius}},
				new TransferConnectData.UnitData{ UnitType = UnitType.Amazon, Skills = new List<Skill>{Skill.VisionRange, Skill.AttackRadius}},
				new TransferConnectData.UnitData{ UnitType = UnitType.Goatman, Skills = new List<Skill>{Skill.VisionRange, Skill.AttackRadius}},
			}
		}, Initialize, TurnDone);
	}

	private void Initialize(TransferInitialData initialData)
	{
		var maze = GetNode<Maze>("Maze");
		this.PlayerId = initialData.YourPlayerId;

		maze.Initialize(initialData.VisibleMap.GetLength(0), initialData.VisibleMap.GetLength(1));
		maze.NewVisibleMap(initialData.VisibleMap);
		maze.Connect(nameof(Maze.CellSelected), this, nameof(CellSelected));

		var unitScene = ResourceLoader.Load<PackedScene>("Unit.tscn");

		foreach (var unit in initialData.YourUnits)
		{
			var unitSceneInstance = (Unit)unitScene.Instance();
			unitSceneInstance.ClientUnit = new ClientUnit
			{
				PlayerId = this.PlayerId,
				UnitId = unit.UnitId,
				UnitType = unit.UnitType,
				MoveDistance = unit.MoveDistance,
				SightRange = unit.SightRange,
				AttackDistance = unit.AttackDistance,
				AttackRadius = unit.AttackRadius,
				MaxHp = unit.MaxHp,
				Hp = unit.MaxHp,
				Usables = unit.Usables.ToDictionary(a => a, a => UnitUtils.FindUsable(a))
			};
			unitSceneInstance.Position = maze.MapToWorld(unit.Position);
			unitSceneInstance.Position += Vector2.Down * maze.CellSize.y / 2;
			unitSceneInstance.AddToGroup(Groups.MyUnits);
			maze.AddChild(unitSceneInstance);
		}

		foreach (var player in initialData.OtherPlayers)
		{
			foreach (var unit in player.Units)
			{
				var unitSceneInstance = (Unit)unitScene.Instance();
				unitSceneInstance.ClientUnit = new ClientUnit
				{
					PlayerId = player.PlayerId,
					UnitId = unit.Id,
					UnitType = unit.UnitType,
					MaxHp = unit.MaxHp,
					Hp = unit.MaxHp
				};
				unitSceneInstance.AddToGroup(Groups.OtherUnits);
				unitSceneInstance.Visible = false;
				maze.AddChild(unitSceneInstance);
			}
		}

		GetNode<Camera2D>("DraggableCamera").Position = maze.MapToWorld(initialData.YourUnits[0].Position) + Vector2.Down * maze.CellSize.y / 2;
	}

	private void UnitActionSelected(CurrentAction action, Usable usable)
	{
		this.currentAction = action;
		this.currentUsable = null;
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
			case CurrentAction.UsableSkill:
				{
					this.currentUsable = currentUnit.ClientUnit.Usables[usable];
					var pos = currentUnit.NewTarget == null ? maze.WorldToMap(currentUnit.Position) : currentUnit.NewTarget.Value;
					this.currentUsable.HighliteMaze(maze, pos, currentUnit.ClientUnit);
					break;
				}
		}
	}

	public void CellSelected(Vector2 cell, Vector2 cellPosition, bool moveAvailable)
	{
		var maze = GetNode<Maze>("Maze");
		var units = this.GetTree().GetNodesInGroup(Groups.MyUnits).Cast<Unit>().ToList();

		var currentUnit = units.FirstOrDefault(a => a.IsSelected);
		var unitActions = GetNode<UnitActions>("UnitActions");

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
					if (unitActions.Visible)
					{
						unitActions.Usables = clickOnUnit.ClientUnit.Usables.Select(a => a.Value.Name).ToList();
					}

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
			case CurrentAction.UsableSkill:
				{
					if (moveAvailable)
					{
						this.currentAction = CurrentAction.None;
						currentUnit.UsableShadowTo(cell, currentUsable);
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
			UnitActions = myUnits.ToDictionary(a => a.ClientUnit.UnitId, a => new TransferTurnDoneData.UnitActionData
			{
				Move = a.NewTarget,
				UsableTarget = a.UsableDirection,
				Usable = a.Usable?.Name ?? Usable.None
			})
		});
	}

	private async void TurnDone(TransferTurnData turnData)
	{
		var maze = GetNode<Maze>("Maze");
		maze.NewVisibleMap(turnData.VisibleMap);
		var myUnits = this.GetTree().GetNodesInGroup(Groups.MyUnits).Cast<Unit>().ToList();
		var otherUnits = this.GetTree().GetNodesInGroup(Groups.OtherUnits).Cast<Unit>().ToList();
		var unitsToHide = otherUnits.Where(a => !turnData.OtherPlayers[a.ClientUnit.PlayerId].Units.ContainsKey(a.ClientUnit.UnitId));
		var visibleUnits = otherUnits.Where(a => turnData.OtherPlayers[a.ClientUnit.PlayerId].Units.ContainsKey(a.ClientUnit.UnitId));
		
		foreach (var unitToHide in unitsToHide)
		{
			unitToHide.Visible = false;
		}

		foreach (var unitToShow in visibleUnits)
		{
			if (!unitToShow.Visible)
			{
				var player = turnData.OtherPlayers[unitToShow.ClientUnit.PlayerId];
				unitToShow.Visible = true;
				unitToShow.Position = maze.MapToWorld(player.Units[unitToShow.ClientUnit.UnitId].Position);
			}
		}

		var signals = new List<SignalAwaiter>();
		
		foreach (var unit in myUnits)
		{
			await unit.MoveUnitTo(turnData.YourUnits[unit.ClientUnit.UnitId].Position);
		}

		foreach (var unit in visibleUnits)
		{
			var player = turnData.OtherPlayers[unit.ClientUnit.PlayerId];
			await unit.MoveUnitTo(player.Units[unit.ClientUnit.UnitId].Position);
		}

		foreach(var signal in signals)
		{
			await signal;
		}
		signals.Clear();

		foreach (var unit in myUnits)
		{
			signals.Add(unit.Attack(turnData.YourUnits[unit.ClientUnit.UnitId].AttackDirection));
		}

		foreach (var unit in visibleUnits)
		{
			var player = turnData.OtherPlayers[unit.ClientUnit.PlayerId];
			signals.Add(unit.Attack(player.Units[unit.ClientUnit.UnitId].AttackDirection));
		}

		foreach (var signal in signals)
		{
			await signal;
		}
		signals.Clear();

		foreach (var unit in myUnits)
		{
			signals.Add(unit.UnitHit(turnData.YourUnits[unit.ClientUnit.UnitId].AttackFrom, turnData.YourUnits[unit.ClientUnit.UnitId].Hp));
		}

		foreach (var unit in visibleUnits)
		{
			var player = turnData.OtherPlayers[unit.ClientUnit.PlayerId];
			signals.Add(unit.UnitHit(player.Units[unit.ClientUnit.UnitId].AttackFrom, player.Units[unit.ClientUnit.UnitId].Hp));
		}

		foreach (var signal in signals)
		{
			await signal;
		}
		signals.Clear();
		
		GetNode<Button>("CanvasLayer/NextTurnButton").Visible = true;
	}
}
