using FateRandom;
using Godot;
using IsometricGame.Logic;
using IsometricGame.Logic.Models;
using System;
using System.Collections.Generic;

public class Bot : Node
{
	private TransferInitialData initialData;
	private Server server;
	private bool waitForMove = false;

	public void NewGame(Server server)
	{
		this.server = server;
		this.server.Connect("Bot", Initialize, TurnDone);
	}

	private void Initialize(TransferInitialData initialData)
	{
		this.initialData = initialData;
		this.waitForMove = true;
	}

	private void TurnDone(TransferTurnData obj)
	{
		this.waitForMove = true;
	}

	public override void _Process(float delta)
	{
		base._Process(delta);

		if (!this.waitForMove)
		{
			return;
		}

		this.waitForMove = false;
		var otherMoves = new Dictionary<int, TransferTurnDoneUnit>();
		foreach (var u in this.initialData.YourUnits)
		{
			otherMoves.Add(u.UnitId, new TransferTurnDoneUnit
			{
				Move = u.Position + Fate.GlobalFate.Choose(Vector2.Up, Vector2.Down, Vector2.Left, Vector2.Right),
				Attack = Fate.GlobalFate.Choose(Vector2.Up, Vector2.Down, Vector2.Left, Vector2.Right)
			});
		}

		this.server.PlayerMove(initialData.YourPlayerId, new TransferTurnDoneData
		{
			UnitActions = otherMoves
		});
	}
}
