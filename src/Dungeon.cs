using Godot;
using IsometricGame.Logic;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using IsometricGame.Logic.ScriptHelpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

public class Dungeon : Node2D
{
	private CurrentAction currentAction = CurrentAction.None;
	private IAbility currentAbility = null;
	private Server serverOptional;

	public override void _Ready()
	{
		GetNode<Button>("CanvasLayer/NextTurnButton").Connect("pressed", this, nameof(NextTurnPressed));
		GetNode<UnitActions>("UnitActions").Connect(nameof(UnitActions.ActionSelected), this, nameof(UnitActionSelected));
	}

	public override void _Process(float delta)
	{
		base._Process(delta);
		if (GetTree().NetworkPeer is WebSocketServer server && server.IsListening())
		{
			server.Poll();
		}
		else if (GetTree().NetworkPeer is WebSocketClient client &&
		   (
		   client.GetConnectionStatus() == NetworkedMultiplayerPeer.ConnectionStatus.Connected ||
		   client.GetConnectionStatus() == NetworkedMultiplayerPeer.ConnectionStatus.Connecting
		   ))
		{
			client.Poll();
		}
	}

	public void NewGame(int selectedTeam, Server server)
	{
		this.serverOptional = server;
		var data = TransferConnectData.Load()[selectedTeam];
		GD.Print($"Local Units count {data.Units.Count}");
		RpcId(1, nameof(ConnectToServer), JsonConvert.SerializeObject(data));
	}
	
	[RemoteSync]
	private void ConnectToServer(string data)
	{
		TransferConnectData connectData = JsonConvert.DeserializeObject<TransferConnectData>(data);
		var clientId = GetTree().GetRpcSenderId();
		GD.Print($"Connected to server with id {clientId}");
		GD.Print($"Units count {connectData.Units.Count}");
		this.serverOptional.Connect(clientId, connectData, 
			(initData) => { RpcId(clientId, nameof(Initialize), JsonConvert.SerializeObject(initData)); }, 
			(turnData) => { RpcId(clientId, nameof(TurnDone), JsonConvert.SerializeObject(turnData)); });
	}

	[RemoteSync]
	private void Initialize(string data)
	{
		TransferInitialData initialData = JsonConvert.DeserializeObject<TransferInitialData>(data);
		GD.Print($"Initialize received");
		var maze = GetNode<Maze>("Maze");

		maze.Initialize(initialData.VisibleMap.GetLength(0), initialData.VisibleMap.GetLength(1));
		maze.NewVisibleMap(initialData.VisibleMap);
		maze.Connect(nameof(Maze.CellSelected), this, nameof(CellSelected));

		var unitScene = ResourceLoader.Load<PackedScene>("Unit.tscn");

		foreach (var unit in initialData.YourUnits)
		{
			var unitSceneInstance = (Unit)unitScene.Instance();
			unitSceneInstance.ClientUnit = new ClientUnit
			{
				PlayerId = 0,
				UnitId = unit.UnitId,
				UnitType = unit.UnitType,
				MoveDistance = unit.MoveDistance,
				SightRange = unit.SightRange,
				AttackDistance = unit.AttackDistance,
				AttackRadius = unit.AttackRadius,
				MaxHp = unit.MaxHp,
				Hp = unit.MaxHp,
				Abilities = unit.Abilities.ToDictionary(a => a, a => UnitUtils.FindAbility(a))
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
					this.currentAbility = currentUnit.ClientUnit.Abilities[ability];
					var pos = currentUnit.NewTarget == null ? maze.WorldToMap(currentUnit.Position) : currentUnit.NewTarget.Value;
					this.currentAbility.HighliteMaze(maze, pos, currentUnit.ClientUnit);
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

					GetNode<UnitDetailsCollapse>("CanvasLayer/UnitDetailsCollapse").SelectUnit(clickOnUnit?.ClientUnit);
					unitActions.Visible = clickOnUnit != null && !clickOnUnit.IsDead;
					unitActions.RectPosition = this.GetViewport().CanvasTransform.AffineInverse().Xform(GetViewport().GetMousePosition());
					if (unitActions.Visible)
					{
						unitActions.Abilities = clickOnUnit.ClientUnit.Abilities.Select(a => a.Value.Name).ToList();
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
			case CurrentAction.UseAbility:
				{
					if (moveAvailable)
					{
						this.currentAction = CurrentAction.None;
						currentUnit.AbilityShadowTo(cell, currentAbility);
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

		RpcId(1, nameof(PlayerMoved), JsonConvert.SerializeObject(new TransferTurnDoneData
		{
			UnitActions = myUnits.ToDictionary(a => a.ClientUnit.UnitId, a => new TransferTurnDoneData.UnitActionData
			{
				Move = a.NewTarget,
				AbilityTarget = a.AbilityDirection,
				Ability = a.Ability?.Name ?? Ability.None
			})
		}));

	}

	[RemoteSync]
	private void PlayerMoved(string data)
	{
		TransferTurnDoneData turnData = JsonConvert.DeserializeObject<TransferTurnDoneData>(data);
		var clientId = GetTree().GetRpcSenderId();

		GD.Print($"Player with id {clientId} moved.");
		serverOptional.PlayerMove(clientId, turnData);
	}

	[RemoteSync]
	private async void TurnDone(string data)
	{
		TransferTurnData turnData = JsonConvert.DeserializeObject<TransferTurnData>(data);
		GD.Print($"Turn done received.");
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
