using FateRandom;
using Godot;
using IsometricGame.Logic;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using IsometricGame.Logic.ScriptHelpers;
using System.Collections.Generic;

public class EasyBot: IBot
{
    private GameLogic gameLogic;

    public EasyBot(GameLogic gameLogic)
    {
		this.gameLogic = gameLogic;
    }

    public Bot Bot => Bot.Easy;

    public void StartGame(GameData gameData, int myId)
	{
		TransferInitialData initData = null;
		this.gameLogic.Connect(gameData, myId, new TransferConnectData
		{
			TeamName = $"Bot-{myId}",
			Units = new List<TransferConnectData.UnitData>
			{
				new TransferConnectData.UnitData{ UnitType = UnitType.Amazon, Skills = new List<Skill>{Skill.EagleEye}},
				new TransferConnectData.UnitData{ UnitType = UnitType.Goatman, Skills = new List<Skill>{Skill.EagleEye}},
				new TransferConnectData.UnitData{ UnitType = UnitType.Amazon, Skills = new List<Skill>{Skill.EagleEye}},
				new TransferConnectData.UnitData{ UnitType = UnitType.Goatman, Skills = new List<Skill>{Skill.EagleEye}},
			}
		}, 
		(initialData) => { initData = initialData; }, 
		(turnData) => TurnDone(initData, gameData, myId, turnData));
	}

	private void TurnDone(TransferInitialData initData, GameData gameData, int myId, TransferTurnData turnData)
	{
		var otherMoves = new Dictionary<int, List<TransferTurnDoneData.UnitActionData>>();
		foreach (var u in initData.YourUnits)
		{
            otherMoves.Add(u.UnitId, new List<TransferTurnDoneData.UnitActionData>
			{
				new TransferTurnDoneData.UnitActionData
				{
					AbilityDirection = Fate.GlobalFate.Choose(Vector2.Up, Vector2.Down, Vector2.Left, Vector2.Right),
					Ability = Ability.Move
				},

				new TransferTurnDoneData.UnitActionData
				{
					AbilityDirection = Fate.GlobalFate.Choose(Vector2.Up, Vector2.Down, Vector2.Left, Vector2.Right),
					Ability = (u.UnitType == UnitType.Amazon) ? Ability.RangedAttack : Ability.MeleeAttack
				}
			});
		}

		gameLogic.PlayerMove(gameData, myId, new TransferTurnDoneData
		{
			UnitActions = otherMoves
		});
	}
}