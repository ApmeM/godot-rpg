using FateRandom;
using Godot;
using IsometricGame.Logic;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using System.Collections.Generic;

public class Bot
{
	private TransferInitialData initialData;
    private string lobbyId;
    private int myId;

	public void NewGame(string lobbyId, int myId)
	{
		this.lobbyId = lobbyId;
		this.myId = myId;
		Server.Connect(lobbyId, myId, new TransferConnectData
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
		_Process();
	}

	private void TurnDone(TransferTurnData obj)
	{
		_Process();
	}

	public void _Process()
	{
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

		Server.PlayerMove(lobbyId, myId, new TransferTurnDoneData
		{
			UnitActions = otherMoves
		});
	}
}
