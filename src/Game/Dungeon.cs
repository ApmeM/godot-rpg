using Godot;
using IsometricGame.Logic;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using IsometricGame.Logic.ScriptHelpers;
using System.Collections.Generic;
using System.Linq;

public class Dungeon : Node2D
{
	private CurrentAction currentAction = CurrentAction.None;
	private Ability? currentAbility = null;
	private string lobbyId;

	[Export]
	public PackedScene UnitScene;

	public override void _Ready()
	{
		GetNode<Button>("CanvasLayer/NextTurnButton").Connect("pressed", this, nameof(NextTurnPressed));
		GetNode<UnitActions>("UnitActions").Connect(nameof(UnitActions.ActionSelected), this, nameof(UnitActionSelected));
	}

	public void NewGame(int selectedTeam, string lobbyId)
	{
		this.lobbyId = lobbyId;
		var data = TransferConnectData.Load()[selectedTeam];
		GetNode<Communicator>("/root/Communicator").ConnectToServer(lobbyId, data);
	}

	public void Initialize(TransferInitialData initialData)
	{
		var maze = GetNode<Maze>("Maze");

		maze.Initialize(initialData.VisibleMap.GetLength(0), initialData.VisibleMap.GetLength(1));
		maze.NewVisibleMap(initialData.VisibleMap);
		maze.Connect(nameof(Maze.CellSelected), this, nameof(CellSelected));

		foreach (var unit in initialData.YourUnits)
		{
			var unitSceneInstance = (Unit)UnitScene.Instance();
			
			unitSceneInstance.ClientUnit = new ClientUnit
			{
				PlayerId = initialData.YourPlayerId,
				UnitId = unit.UnitId,
				UnitType = unit.UnitType,
				MoveDistance = unit.MoveDistance,
				SightRange = unit.SightRange,
				RangedAttackDistance = unit.RangedAttackDistance,
				AOEAttackRadius = unit.AOEAttackRadius,
				MaxHp = unit.MaxHp,
				Hp = unit.MaxHp,
				Abilities = unit.Abilities.ToDictionary(a => a, a => UnitUtils.FindAbility(a)),
				Skills = unit.Skills.ToHashSet()
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
				var unitSceneInstance = (Unit)UnitScene.Instance();
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

	private void UnitActionSelected(CurrentAction action, Ability ability)
	{
		this.currentAction = action;
		this.currentAbility = null;
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
			case CurrentAction.UseAbility:
				{
					this.currentAbility = ability;
					var pos = currentUnit.NewPosition == null ? maze.WorldToMap(currentUnit.Position) : currentUnit.NewPosition.Value;
					UnitUtils.FindAbility(ability).HighliteMaze(maze, pos, currentUnit.ClientUnit);
					break;
				}
		}
	}

	public void CellSelected(Vector2 cell, Vector2 cellPosition, bool moveAvailable)
	{
		var maze = GetNode<Maze>("Maze");
		var myUnits = this.GetTree().GetNodesInGroup(Groups.MyUnits).Cast<Unit>().ToList();
		var otherUnits = this.GetTree().GetNodesInGroup(Groups.OtherUnits).Cast<Unit>().ToList();

		var currentUnit = myUnits.FirstOrDefault(a => a.IsSelected);
		var unitActions = GetNode<UnitActions>("UnitActions");

		switch (this.currentAction)
		{
			case CurrentAction.None:
				{
					var clickOnUnit = myUnits.FirstOrDefault(a => maze.WorldToMap(a.Position) == cell || a.NewPosition == cell);
					if (currentUnit != null)
					{
						currentUnit.IsSelected = false;
					}

					unitActions.Visible = clickOnUnit != null && !clickOnUnit.IsDead;
					unitActions.RectPosition = this.GetViewport().CanvasTransform.AffineInverse().Xform(GetViewport().GetMousePosition());
					if (unitActions.Visible)
					{
						unitActions.Abilities = clickOnUnit.ClientUnit.Abilities.Select(a => a.Key).ToList();
					}
					GetNode<UnitDetailsCollapse>("CanvasLayer/UnitDetailsCollapse").SelectUnit(clickOnUnit?.ClientUnit);

					if (clickOnUnit != null)
					{
						clickOnUnit.IsSelected = true;
					}
					else
					{
						var enemyUnit = otherUnits.FirstOrDefault(a => maze.WorldToMap(a.Position) == cell);
						if (enemyUnit != null)
						{
							GetNode<UnitDetailsCollapse>("CanvasLayer/UnitDetailsCollapse").SelectUnit(enemyUnit.ClientUnit);
						}
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
			case CurrentAction.UseAbility:
				{
					if (moveAvailable)
					{
						var ability = UnitUtils.FindAbility(currentAbility.Value);

						this.currentAction = CurrentAction.None;
						Unit target = null;
						if (ability.TargetUnit)
						{
							target = myUnits
								.Union(otherUnits)
								.Where(unit => (unit.NewPosition == null ? maze.WorldToMap(unit.Position) : unit.NewPosition.Value) == cell)
								.FirstOrDefault();
						}
						currentUnit.AbilityShadowTo(currentAbility.Value, cell, target);
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
		
        var data = new TransferTurnDoneData
        {
            UnitActions = myUnits.ToDictionary(a => a.ClientUnit.UnitId, a => new TransferTurnDoneData.UnitActionData
            {
                Move = a.NewPosition,
                Ability = a.Ability ?? Ability.None,
                AbilityDirection = a.AbilityDirection,
                AbilityFullUnitId = a.AbilityUnitTarget == null ? -1 : UnitUtils.GetFullUnitId(a.AbilityUnitTarget.ClientUnit.PlayerId, a.AbilityUnitTarget.ClientUnit.UnitId)
            })
        };


		GetNode<Communicator>("/root/Communicator").PlayerMoved(lobbyId, data);

	}

	public async void TurnDone(TransferTurnData turnData)
	{
		var maze = GetNode<Maze>("Maze");
		maze.NewVisibleMap(turnData.VisibleMap);
		var myUnits = this.GetTree().GetNodesInGroup(Groups.MyUnits).Cast<Unit>().ToList();
		var otherUnits = this.GetTree().GetNodesInGroup(Groups.OtherUnits).Cast<Unit>().ToList();
		var unitsToHide = otherUnits.Where(a => !turnData.OtherPlayers[a.ClientUnit.PlayerId].Units.ContainsKey(a.ClientUnit.UnitId));
		var visibleUnits = otherUnits.Where(a => turnData.OtherPlayers[a.ClientUnit.PlayerId].Units.ContainsKey(a.ClientUnit.UnitId));

		foreach (var unit in myUnits)
		{
			unit.ClientUnit.Effects = turnData.YourUnits[unit.ClientUnit.UnitId].Effects;
			unit.ClientUnit.MoveDistance = turnData.YourUnits[unit.ClientUnit.UnitId].MoveDistance;
			unit.ClientUnit.SightRange = turnData.YourUnits[unit.ClientUnit.UnitId].SightRange;
			unit.ClientUnit.RangedAttackDistance = turnData.YourUnits[unit.ClientUnit.UnitId].RangedAttackDistance;
			unit.ClientUnit.AOEAttackRadius = turnData.YourUnits[unit.ClientUnit.UnitId].AOEAttackRadius;
			unit.ClientUnit.AttackPower = turnData.YourUnits[unit.ClientUnit.UnitId].AttackPower;
			unit.ClientUnit.MagicPower = turnData.YourUnits[unit.ClientUnit.UnitId].MagicPower;
		}

		foreach (var unitToHide in unitsToHide)
		{
			unitToHide.Visible = false;
		}

		foreach (var unitToShow in visibleUnits)
		{
			var player = turnData.OtherPlayers[unitToShow.ClientUnit.PlayerId];

			unitToShow.ClientUnit.Effects = player.Units[unitToShow.ClientUnit.UnitId].Effects;
			if (!unitToShow.Visible)
			{
				unitToShow.Visible = true;
				unitToShow.Position = maze.MapToWorld(player.Units[unitToShow.ClientUnit.UnitId].Position);
			}
		}

		foreach (var unit in visibleUnits)
		{
			var player = turnData.OtherPlayers[unit.ClientUnit.PlayerId];
			await unit.MoveUnitTo(player.Units[unit.ClientUnit.UnitId].Position);
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
