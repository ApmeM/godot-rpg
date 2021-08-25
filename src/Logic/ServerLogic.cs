using BrainAI.Pathfinding.BreadthFirst;
using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using IsometricGame.Logic.ScriptHelpers;
using IsometricGame.Logic.ScriptHelpers.AppliedActions;
using MazeGenerators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IsometricGame.Logic
{
	public class ServerLogic
	{
		private static readonly TransferTurnDoneData emptyMoves = new TransferTurnDoneData();

		public GameData Start(ServerConfiguration configuration)
		{
			var result = new GameData();
			var generator = new RoomMazeGenerator();
			result.Configuration = configuration;
			result.Map = generator.Generate(new RoomMazeGenerator.Settings
			{
				Width = configuration.PlayersCount * 10 + 1,
				Height = configuration.PlayersCount * 10 + 1,
				MinRoomSize = 5,
				MaxRoomSize = 9,
			});

			result.Astar = new VectorGridGraph(result.Map.Regions.GetLength(0), result.Map.Regions.GetLength(1));
			for (var x = 0; x < result.Map.Regions.GetLength(0); x++)
				for (var y = 0; y < result.Map.Regions.GetLength(1); y++)
				{
					if (result.Map.Regions[x, y].HasValue)
					{
						result.Astar.Paths.Add(new Vector2(x, y));
					}
				}

			return result;
		}

		public void Connect(
			GameData gameData,
			int playerId, 
			TransferConnectData connectData,
			Action<TransferInitialData> initialize,
			Action<TransferTurnData> turnDone)
		{
			if (gameData.Players.ContainsKey(playerId))
			{
				throw new Exception($"Player with id {playerId} already connected");
			}

			var player = new ServerPlayer
			{
				PlayerId = playerId,
				PlayerName = connectData.TeamName,
			};

			var playerNumber = gameData.Players.Count();
			var centerX = gameData.Map.Rooms[playerNumber].X + gameData.Map.Rooms[playerNumber].Width / 2;
			var centerY = gameData.Map.Rooms[playerNumber].Y + gameData.Map.Rooms[playerNumber].Height / 2;
			var center = new Vector2(centerX, centerY);
			foreach (var u in connectData.Units.Take(gameData.Configuration.MaxUnits))
			{
				var unitId = player.Units.Count + 1;
				var unit = UnitUtils.BuildUnit(player, u.UnitType, u.Skills.Take(gameData.Configuration.MaxSkills).ToList());
				unit.UnitId = unitId;
				unit.Player = player;
				player.Units.Add(unitId, unit);
			}

			var positions = new List<Vector2>
			{
				Vector2.Left,
				Vector2.Right,
				Vector2.Up,
				Vector2.Down,
				Vector2.Zero
			};

			var index = 0;
			foreach (var unit in player.Units)
			{
				unit.Value.Position = center + positions[index];
				index++;
			}

			gameData.Players.Add(playerId, player);

			gameData.InitializeMethods[playerId] = initialize;
			gameData.TurnDoneMethods[playerId] = turnDone;

			if (playerNumber + 1 == gameData.Configuration.PlayersCount)
			{
				foreach (var p in gameData.Players)
				{
					foreach (var u in p.Value.Units)
					{
						UnitUtils.RefreshUnit(p.Value, u.Value);
					}
				}

				foreach (var initMethod in gameData.InitializeMethods)
				{
					initMethod.Value(GetInitialData(gameData, initMethod.Key));
				}

				foreach (var turnDoneMethod in gameData.TurnDoneMethods)
				{
					turnDoneMethod.Value(GetTurnData(gameData, turnDoneMethod.Key, new Dictionary<long, ServerTurnDelta>()));
				}
			}
		}

		public void PlayerMove(GameData gameData, int forPlayer, TransferTurnDoneData moves)
		{
			if (gameData.PlayersGameOver.ContainsKey(forPlayer))
			{
				return;
			}

			// Put player move into dictionary
			if (!gameData.Players.ContainsKey(forPlayer))
			{
				return;
			}

			gameData.PlayersMove[forPlayer] = moves;

			if (gameData.PlayersMove.Count != gameData.Players.Count)
			{
				return;
			}

			// When all players send their turns - apply all.
			gameData.Timeout = gameData.Configuration.TurnTimeout;
			var unitsTurnDelta = new Dictionary<long, ServerTurnDelta>();
			var occupiedCells = new HashSet<Vector2>();
			var appliedActions = new List<IAppliedAction>();

			/* Initialize turn delta. */
			foreach (var actionPlayer in gameData.Players)
			{
				foreach (var actionUnit in actionPlayer.Value.Units)
				{
					var fullId = UnitUtils.GetFullUnitId(actionUnit.Value);
					unitsTurnDelta[fullId] = new ServerTurnDelta();
					gameData.PlayersMove[actionPlayer.Key].UnitActions = gameData.PlayersMove[actionPlayer.Key].UnitActions ?? new Dictionary<int, TransferTurnDoneData.UnitActionData>();
					if (!gameData.PlayersMove[actionPlayer.Key].UnitActions.ContainsKey(actionUnit.Key))
					{
						gameData.PlayersMove[actionPlayer.Key].UnitActions[actionUnit.Key] = new TransferTurnDoneData.UnitActionData();
					}
				}
			}

			/* Calculate move blockers */
			foreach (var actionPlayer in gameData.Players)
			{
				foreach (var actionUnit in actionPlayer.Value.Units)
				{
					var unitMove = gameData.PlayersMove[actionPlayer.Key].UnitActions[actionUnit.Key];
					if (actionUnit.Value.Hp > 0 && unitMove.Move.HasValue)
					{
						continue;
					}

					occupiedCells.Add(actionUnit.Value.Position);
				}
			}

			/* Increase Hp for units that do not have any commands */
			foreach (var actionPlayer in gameData.Players)
			{
				foreach (var actionUnit in actionPlayer.Value.Units)
				{
					var unitMove = gameData.PlayersMove[actionPlayer.Key].UnitActions[actionUnit.Key];
					if (unitMove.Move.HasValue || actionUnit.Value.Hp <= 0 || unitMove.Ability != Ability.None)
					{
						continue;
					}


					var targetFullId = UnitUtils.GetFullUnitId(actionUnit.Value);
					var targetDelta = unitsTurnDelta[targetFullId];
					appliedActions.Add(new ChangeHpAppliedAction(actionUnit.Value.MaxHp / 10, actionUnit.Value));
					appliedActions.Add(new ChangeMpAppliedAction(actionUnit.Value.MaxMp / 10, actionUnit.Value));
				}
			}

			/* Move units */
			foreach (var actionPlayer in gameData.Players)
			{
				foreach (var actionUnit in actionPlayer.Value.Units)
				{
					var unitMove = gameData.PlayersMove[actionPlayer.Key].UnitActions[actionUnit.Key];
					if (!unitMove.Move.HasValue || actionUnit.Value.Hp <= 0)
					{
						continue;
					}

					var path = BreadthFirstPathfinder.Search(gameData.Astar, actionUnit.Value.Position, unitMove.Move.Value)
						.Take(actionUnit.Value.MoveDistance + 1)
						.ToList();

					for (var i = path.Count; i > 0; i--)
					{
						if (occupiedCells.Contains(path[i - 1]))
						{
							continue;
						}

						actionUnit.Value.Position = path[i - 1];
						occupiedCells.Add(actionUnit.Value.Position);
						break;
					}
				}
			}

			/* Execute abilities */
			foreach (var actionPlayer in gameData.Players)
			{
				foreach (var actionUnit in actionPlayer.Value.Units)
				{
					var unitMove = gameData.PlayersMove[actionPlayer.Key].UnitActions[actionUnit.Key];
					var actionFullId = UnitUtils.GetFullUnitId(actionUnit.Value);
					var actionDelta = unitsTurnDelta[actionFullId];
					if (
						!unitMove.AbilityDirection.HasValue ||
						actionUnit.Value.Hp <= 0 ||
						!actionUnit.Value.Abilities.Contains(unitMove.Ability))
					{
						continue;
					}

					var wasApplied = false;

					var ability = UnitUtils.FindAbility(unitMove.Ability);
					if (ability.TargetUnit)
					{
						var targetPlayerId = UnitUtils.GetPlayerId(unitMove.AbilityFullUnitId);
						var targetUnitId = UnitUtils.GetUnitId(unitMove.AbilityFullUnitId);
						var targetPlayer = gameData.Players[targetPlayerId];
						var targetUnit = targetPlayer.Units[targetUnitId];

						actionDelta.AbilityDirection = targetUnit.Position - actionUnit.Value.Position;
					}
					else
					{
						actionDelta.AbilityDirection = unitMove.AbilityDirection.Value;
						wasApplied = true;

					}

					foreach (var targetPlayer in gameData.Players)
					{
						foreach (var targetUnit in targetPlayer.Value.Units)
						{
							var isApplicable = ability.IsApplicable(gameData.Astar, actionUnit.Value, targetUnit.Value, actionDelta.AbilityDirection.Value);
							if (!isApplicable)
							{
								continue;
							}

							var targetFullId = UnitUtils.GetFullUnitId(targetUnit.Value);
							var targetDelta = unitsTurnDelta[targetFullId];
							targetDelta.AbilityFrom = actionUnit.Value.Position - targetUnit.Value.Position;

							wasApplied = true;

							var actions = ability.Apply(actionUnit.Value, targetUnit.Value);
							appliedActions.AddRange(actions);
						}
					}

					if (wasApplied)
					{
						var costs = ability.ApplyCost(actionUnit.Value);
						appliedActions.AddRange(costs);
					}
				}
			}

			foreach (var action in appliedActions)
			{
				action.Apply();
				if (action is ChangeHpAppliedAction hpAction)
				{
					unitsTurnDelta[hpAction.FullUnitId].HpChanges.Add(hpAction.Value);
				}
				if (action is ChangeMpAppliedAction mpAction)
				{
					unitsTurnDelta[mpAction.FullUnitId].MpChanges.Add(mpAction.Value);
				}
			}
			appliedActions.Clear();

			/* Refresh unit values. */
			foreach (var actionPlayer in gameData.Players)
			{
				foreach (var actionUnit in actionPlayer.Value.Units)
				{
					UnitUtils.RefreshUnit(actionPlayer.Value, actionUnit.Value);
				}
			}

			/* Calculate all effects */
			foreach (var actionPlayer in gameData.Players)
			{
				foreach (var actionUnit in actionPlayer.Value.Units)
				{
					var targetFullId = UnitUtils.GetFullUnitId(actionUnit.Value);
					var targetDelta = unitsTurnDelta[targetFullId];

					foreach (var effect in actionUnit.Value.Effects)
					{
						var actions = UnitUtils.FindEffect(effect.Effect).Apply(actionUnit.Value);
						appliedActions.AddRange(actions);
						effect.Duration--;
					}

					actionUnit.Value.Effects.RemoveAll(a => a.Duration <= 0);
				}
			}

			foreach (var action in appliedActions)
			{
				action.Apply();
				if (action is ChangeHpAppliedAction hpAction)
				{
					unitsTurnDelta[hpAction.FullUnitId].HpChanges.Add(hpAction.Value);
				}
				if (action is ChangeMpAppliedAction mpAction)
				{
					unitsTurnDelta[mpAction.FullUnitId].MpChanges.Add(mpAction.Value);
				}
			}
			appliedActions.Clear();

			/* Check survived units */
			foreach (var actionPlayer in gameData.Players)
			{
				foreach (var actionUnit in actionPlayer.Value.Units)
				{
					actionUnit.Value.Hp = Mathf.Clamp(actionUnit.Value.Hp, 0, actionUnit.Value.MaxHp);
					actionUnit.Value.Mp = Mathf.Clamp(actionUnit.Value.Mp, 0, actionUnit.Value.MaxMp);
				}

				actionPlayer.Value.IsGameOver = actionPlayer.Value.IsGameOver || CheckGameOver(actionPlayer.Value);
			}

			gameData.PlayersMove.Clear();

			foreach (var turnDoneMethod in gameData.TurnDoneMethods)
			{
				turnDoneMethod.Value(GetTurnData(gameData, turnDoneMethod.Key, unitsTurnDelta));
			}

			foreach (var actionPlayer in gameData.Players.ToList())
			{
				if (!actionPlayer.Value.IsGameOver)
				{
					continue;
				}
				gameData.TurnDoneMethods.Remove(actionPlayer.Key);
				gameData.InitializeMethods.Remove(actionPlayer.Key);
				gameData.Players.Remove(actionPlayer.Key);
				gameData.PlayersGameOver[actionPlayer.Key] = actionPlayer.Value;
			}

			// Todo: Remove game if it is over for all players.
		}

		private TransferInitialData GetInitialData(GameData gameData, int forPlayer)
		{
			return new TransferInitialData
			{
				Timeout = gameData.Configuration.TurnTimeout,
				YourPlayerId = forPlayer,
				YourUnits = gameData.Players[forPlayer].Units.Select(a => new TransferInitialData.YourUnitsData
				{
					UnitId = a.Key,
					Position = a.Value.Position,
					MoveDistance = a.Value.MoveDistance,
					SightRange = a.Value.SightRange,
					RangedAttackDistance = a.Value.RangedAttackDistance,
					AOEAttackRadius = a.Value.AOEAttackRadius,
					AttackPower = a.Value.AttackPower,
					MagicPower = a.Value.MagicPower,
					MaxHp = a.Value.MaxHp,
					MaxMp = a.Value.MaxMp,
					UnitType = a.Value.UnitType,
					Abilities = a.Value.Abilities.ToList(),
					Skills = a.Value.Skills.ToList()
				}).ToList(),
				VisibleMap = GetVisibleMap(gameData, forPlayer, true),
				OtherPlayers = gameData.Players.Where(a => a.Key != forPlayer).Select(a => new TransferInitialData.OtherPlayerData
				{
					PlayerId = a.Key,
					PlayerName = a.Value.PlayerName,
					Units = a.Value.Units.Select(b => new TransferInitialData.OtherUnitsData
					{
						Id = b.Key,
						UnitType = b.Value.UnitType,
						MaxHp = b.Value.MaxHp,
					}).ToList(),
				}).ToList()
			};
		}

		private TransferTurnData GetTurnData(GameData gameData, int forPlayer, Dictionary<long, ServerTurnDelta> UnitsTurnDelta)
		{
			var player = gameData.Players[forPlayer];
			return new TransferTurnData
			{
				GameOverLoose = player.IsGameOver,
				GameOverWin = gameData.Players.Where(a => a.Key != forPlayer).All(a => a.Value.IsGameOver),
				YourUnits = player.Units.ToDictionary(a => a.Key, a =>
				{
					var fullId = UnitUtils.GetFullUnitId(a.Value);
					UnitsTurnDelta.TryGetValue(fullId, out var delta);
					return new TransferTurnData.YourUnitsData
					{
						Position = a.Value.Position,
						AbilityDirection = a.Value.Hp <= 0 ? null : delta?.AbilityDirection,
						Hp = a.Value.Hp,
						Mp = a.Value.Mp,
						AbilityFrom = a.Value.Hp <= 0 ? null : delta?.AbilityFrom,
						Effects = a.Value.Effects,
						MoveDistance = a.Value.MoveDistance,
						SightRange = a.Value.SightRange,
						RangedAttackDistance = a.Value.RangedAttackDistance,
						AOEAttackRadius = a.Value.AOEAttackRadius,
						AttackPower = a.Value.AttackPower,
						MagicPower = a.Value.MagicPower,
						HpChanges = delta?.HpChanges,
						MpChanges = delta?.MpChanges,
					};
				}),
				VisibleMap = this.GetVisibleMap(gameData, forPlayer, false),
				OtherPlayers = gameData.Players.Where(a => a.Key != forPlayer).ToDictionary(a => a.Key, a => new TransferTurnData.OtherPlayersData
				{
					Units = a.Value.Units.Where(b => IsVisible(player, (int)b.Value.Position.x, (int)b.Value.Position.y)).ToDictionary(b => b.Key, b =>
					{
						var fullId = UnitUtils.GetFullUnitId(b.Value);
						UnitsTurnDelta.TryGetValue(fullId, out var delta);

						return new TransferTurnData.OtherUnitsData
						{
							Position = b.Value.Position,
							AttackDirection = b.Value.Hp <= 0 ? null : delta?.AbilityDirection,
							Hp = b.Value.Hp,
							AttackFrom = b.Value.Hp <= 0 ? null : delta?.AbilityFrom,
							Effects = b.Value.Effects,
							HpChanges = delta?.HpChanges,
						};
					})
				})
			};
		}

		private bool CheckGameOver(ServerPlayer player)
		{
			return player.Units.All(unit => unit.Value.Hp <= 0);
		}

		private MapTile[,] GetVisibleMap(GameData gameData, int forPlayer, bool isInitialize)
		{
			var player = gameData.Players[forPlayer];

			var result = new MapTile[gameData.Map.Regions.GetLength(0), gameData.Map.Regions.GetLength(1)];
			for (var x = 0; x < gameData.Map.Regions.GetLength(0); x++)
				for (var y = 0; y < gameData.Map.Regions.GetLength(1); y++)
				{
					if (!IsVisible(player, x, y) && (!gameData.Configuration.FullMapVisible || !isInitialize))
					{
						result[x, y] = MapTile.Unknown;
					}
					else if (gameData.Map.Regions[x, y].HasValue)
					{
						result[x, y] = MapTile.Path;
					}
					else
					{
						result[x, y] = MapTile.Wall;
					}
				}

			return result;
		}

		private static bool IsVisible(ServerPlayer player, int x, int y)
		{
			foreach (var unit in player.Units.Values)
			{
				if (unit.Hp <= 0)
				{
					continue;
				}

				if ((Math.Abs(x - unit.Position.x) + Math.Abs(y - unit.Position.y)) <= unit.SightRange)
				{
					return true;
				}
			}

			return false;
		}

		public void CheckTimeout(GameData gameData, float delta)
		{
			if (!gameData.Configuration.TurnTimeout.HasValue)
			{
				return;
			}

			gameData.Timeout = gameData.Timeout ?? gameData.Configuration.TurnTimeout;
			gameData.Timeout -= delta;
			if (gameData.Timeout > 0)
			{
				return;
			}

			gameData.Timeout = gameData.Configuration.TurnTimeout;
			foreach (var player in gameData.Players)
			{
				if (gameData.PlayersMove.ContainsKey(player.Key))
				{
					continue;
				}

				this.PlayerMove(gameData, player.Key, emptyMoves);
			}
		}
	}
}
