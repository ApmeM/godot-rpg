using FateRandom;
using Godot;
using IsometricGame.Logic;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using System.Collections.Generic;

public class Bot : Node
{
	private TransferInitialData initialData;
    private int myId;
    private Server server;
	private bool waitForMove = false;

	public void NewGame(Server server, int myId)
	{
		this.myId = myId;
		this.server = server;
		this.server.Connect(myId, new TransferConnectData
		{
			TeamName = $"Bot-{myId}",
			Units = new List<TransferConnectData.UnitData>
			{
				new TransferConnectData.UnitData{ UnitType = UnitType.Amazon, Skills = new List<Skill>{Skill.EagleEye}},
				new TransferConnectData.UnitData{ UnitType = UnitType.Goatman, Skills = new List<Skill>{Skill.EagleEye}},
				new TransferConnectData.UnitData{ UnitType = UnitType.Amazon, Skills = new List<Skill>{Skill.EagleEye}},
				new TransferConnectData.UnitData{ UnitType = UnitType.Goatman, Skills = new List<Skill>{Skill.EagleEye}},
			}
		}, Initialize, TurnDone);
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
		var otherMoves = new Dictionary<int, TransferTurnDoneData.UnitActionData>();
		foreach (var u in this.initialData.YourUnits)
		{
			otherMoves.Add(u.UnitId, new TransferTurnDoneData.UnitActionData
			{
				Move = u.Position + Fate.GlobalFate.Choose(Vector2.Up, Vector2.Down, Vector2.Left, Vector2.Right),
				AbilityDirection = Fate.GlobalFate.Choose(Vector2.Up, Vector2.Down, Vector2.Left, Vector2.Right),
				Ability = (u.UnitType == UnitType.Amazon) ? Ability.RangedAttack : Ability.MeleeAttack
			});
		}

		this.server.PlayerMove(myId, new TransferTurnDoneData
		{
			UnitActions = otherMoves
		});
	}
}
