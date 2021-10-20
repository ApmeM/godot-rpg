using BrainAI.Pathfinding.BreadthFirst;
using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using IsometricGame.Logic.ScriptHelpers;
using IsometricGame.Logic.ScriptHelpers.AppliedActions;
using System;
using System.Collections.Generic;
using System.Linq;
using Vector2 = Godot.Vector2;

namespace IsometricGame.Logic
{
	public class GameLogic
	{
		private static readonly TransferTurnDoneData emptyMoves = new TransferTurnDoneData();
        private readonly MapRepository mapRepository;
        private readonly PluginUtils pluginUtils;
        private readonly UnitUtils unitUtils;

        public GameLogic(MapRepository mapRepository, PluginUtils pluginUtils, UnitUtils unitUtils)
        {
            this.mapRepository = mapRepository;
            this.pluginUtils = pluginUtils;
            this.unitUtils = unitUtils;
        }

		public GameData StartForLobby(LobbyData lobby)
		{
			var game = new GameData(lobby.Id);
			game.Configuration = lobby.ServerConfiguration;
			game.Map = this.mapRepository.CreateForType(lobby.ServerConfiguration.MapType);
			game.Astar = new MapGraphData(game.Map.Paths.GetLength(0), game.Map.Paths.GetLength(1));
			for (var x = 0; x < game.Map.Paths.GetLength(0); x++)
				for (var y = 0; y < game.Map.Paths.GetLength(1); y++)
				{
					switch(game.Map.Paths[x, y])
					{
						case MapRepository.JunctionTileId:
						case MapRepository.MazeTileId:
						case MapRepository.RoomTileId:
							{
								game.Astar.Paths.Add(new Vector2(x, y));
								break;
							}
					}
				}

			List<Tuple<IBot, int>> bots = new List<Tuple<IBot, int>>();
			var botNumber = 1;
			for (var i = 0; i < lobby.Players.Count; i++)
            {
                var player = lobby.Players[i];
                var playerId = player.ClientId;
                if (playerId < 0)
				{
					var bot = pluginUtils.FindBot((Bot)player.ClientId);
					bots.Add(new Tuple<IBot, int>(bot, -botNumber));
					playerId = -botNumber;
					botNumber++;
				}

                game.Players.Add(playerId, new ServerPlayer
                {
					PlayerId = playerId,
					PlayerName = lobby.Players[i].PlayerName
				});
			}
			
			for (var i = 0; i < bots.Count; i++)
            {
				var bot = bots[i];
				bot.Item1.StartGame(game, bot.Item2);
            }

			return game;
		}

		public bool Connect(
			GameData game,
			int playerId,
			TransferConnectData connectData,
			Action<TransferInitialData> initialize,
			Action<TransferTurnData> turnDone)
		{
			if (!game.Players.ContainsKey(playerId))
			{
				return false;
			}

			var player = game.Players[playerId];
			if (player.IsConnected)
			{
				return true;
			}

			player.IsConnected = true;
			player.InitializeMethod = initialize;
			player.TurnDoneMethod = turnDone;

			var connectedPlayers = game.Players.Count(a => a.Value.IsConnected);
			var centerX = game.Map.Rooms[connectedPlayers - 1].X + game.Map.Rooms[connectedPlayers - 1].Width / 2;
			var centerY = game.Map.Rooms[connectedPlayers - 1].Y + game.Map.Rooms[connectedPlayers - 1].Height / 2;
			var center = new Vector2(centerX, centerY);
			foreach (var u in connectData.Units.Take(game.Configuration.MaxUnits))
			{
				var unitId = player.Units.Count + 1;
				var unit = this.unitUtils.BuildUnit(player, u.UnitType, u.Skills.Take(game.Configuration.MaxSkills).ToList());
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

			if (connectedPlayers == game.Players.Count())
			{
				foreach (var p in game.Players)
				{
					foreach (var u in p.Value.Units)
					{
						this.unitUtils.RefreshUnit(p.Value, u.Value);
					}
				}

				foreach (var p in game.Players)
				{
					p.Value.InitializeMethod(GetInitialData(game, p.Key));
				}

				foreach (var p in game.Players)
				{
					p.Value.TurnDoneMethod(GetTurnData(game, p.Key, new Dictionary<long, ServerTurnDelta>()));
				}
			}

			return true;
		}

		public void PlayerMove(GameData game, int forPlayer, TransferTurnDoneData moves)
		{
			if (game.PlayersGameOver.ContainsKey(forPlayer))
			{
				return;
			}

			// Put player move into dictionary
			if (!game.Players.ContainsKey(forPlayer))
			{
				return;
			}

			game.PlayersMove[forPlayer] = moves;

			if (game.PlayersMove.Count != game.Players.Count)
			{
				return;
			}

			// When all players send their turns - apply all.
			game.Timeout = game.Configuration.TurnTimeout;
			var unitsTurnDelta = new Dictionary<long, ServerTurnDelta>();
			var occupiedCells = new HashSet<Vector2>();
			var appliedActions = new List<IAppliedAction>();

			/* Initialize turn delta. */
			foreach (var actionPlayer in game.Players)
			{
				foreach (var actionUnit in actionPlayer.Value.Units)
				{
					var fullId = UnitUtils.GetFullUnitId(actionUnit.Value);
					unitsTurnDelta[fullId] = new ServerTurnDelta();
					game.PlayersMove[actionPlayer.Key].UnitActions = game.PlayersMove[actionPlayer.Key].UnitActions ?? new Dictionary<int, TransferTurnDoneData.UnitActionData>();
					if (!game.PlayersMove[actionPlayer.Key].UnitActions.ContainsKey(actionUnit.Key))
					{
						game.PlayersMove[actionPlayer.Key].UnitActions[actionUnit.Key] = new TransferTurnDoneData.UnitActionData();
					}
				}
			}

			/* Calculate move blockers */
			foreach (var actionPlayer in game.Players)
			{
				foreach (var actionUnit in actionPlayer.Value.Units)
				{
					var unitMove = game.PlayersMove[actionPlayer.Key].UnitActions[actionUnit.Key];
					if (actionUnit.Value.Hp > 0 && unitMove.Move.HasValue)
					{
						continue;
					}

					occupiedCells.Add(actionUnit.Value.Position);
				}
			}

			/* Increase Hp for units that do not have any commands */
			foreach (var actionPlayer in game.Players)
			{
				foreach (var actionUnit in actionPlayer.Value.Units)
				{
					var unitMove = game.PlayersMove[actionPlayer.Key].UnitActions[actionUnit.Key];
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
			foreach (var actionPlayer in game.Players)
			{
				foreach (var actionUnit in actionPlayer.Value.Units)
				{
					var unitMove = game.PlayersMove[actionPlayer.Key].UnitActions[actionUnit.Key];
					if (!unitMove.Move.HasValue || actionUnit.Value.Hp <= 0)
					{
						continue;
					}

					var path = BreadthFirstPathfinder.Search(game.Astar, actionUnit.Value.Position, unitMove.Move.Value)
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
			foreach (var actionPlayer in game.Players)
			{
				foreach (var actionUnit in actionPlayer.Value.Units)
				{
					var unitMove = game.PlayersMove[actionPlayer.Key].UnitActions[actionUnit.Key];
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

					var ability = pluginUtils.FindAbility(unitMove.Ability);
					if (ability.TargetUnit)
					{
						var targetPlayerId = UnitUtils.GetPlayerId(unitMove.AbilityFullUnitId);
						var targetUnitId = UnitUtils.GetUnitId(unitMove.AbilityFullUnitId);
						var targetPlayer = game.Players[targetPlayerId];
						var targetUnit = targetPlayer.Units[targetUnitId];

						actionDelta.AbilityDirection = targetUnit.Position - actionUnit.Value.Position;
					}
					else
					{
						actionDelta.AbilityDirection = unitMove.AbilityDirection.Value;
						wasApplied = true;

					}

					foreach (var targetPlayer in game.Players)
					{
						foreach (var targetUnit in targetPlayer.Value.Units)
						{
							var isApplicable = ability.IsApplicable(game.Astar, actionUnit.Value, targetUnit.Value, actionDelta.AbilityDirection.Value);
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
			foreach (var actionPlayer in game.Players)
			{
				foreach (var actionUnit in actionPlayer.Value.Units)
				{
					this.unitUtils.RefreshUnit(actionPlayer.Value, actionUnit.Value);
				}
			}

			/* Calculate all effects */
			foreach (var actionPlayer in game.Players)
			{
				foreach (var actionUnit in actionPlayer.Value.Units)
				{
					var targetFullId = UnitUtils.GetFullUnitId(actionUnit.Value);
					var targetDelta = unitsTurnDelta[targetFullId];

					foreach (var effect in actionUnit.Value.Effects)
					{
						var actions = pluginUtils.FindEffect(effect.Effect).Apply(actionUnit.Value);
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
			foreach (var actionPlayer in game.Players)
			{
				foreach (var actionUnit in actionPlayer.Value.Units)
				{
					actionUnit.Value.Hp = Mathf.Clamp(actionUnit.Value.Hp, 0, actionUnit.Value.MaxHp);
					actionUnit.Value.Mp = Mathf.Clamp(actionUnit.Value.Mp, 0, actionUnit.Value.MaxMp);
				}

				actionPlayer.Value.IsGameOver = actionPlayer.Value.IsGameOver || CheckGameOver(actionPlayer.Value);
			}

			game.PlayersMove.Clear();

			foreach (var p in game.Players)
			{
				p.Value.TurnDoneMethod(GetTurnData(game, p.Key, unitsTurnDelta));
			}

			foreach (var actionPlayer in game.Players.ToList())
			{
				if (!actionPlayer.Value.IsGameOver)
				{
					continue;
				}
				game.Players.Remove(actionPlayer.Key);
				game.PlayersGameOver[actionPlayer.Key] = actionPlayer.Value;
			}
		}

		public void PlayerExitGame(GameData gameData, int clientId)
		{
			foreach (var unit in gameData.Players[clientId].Units)
			{
				unit.Value.Hp = 0;
			}

			PlayerMove(gameData, clientId, emptyMoves);
		}

		private TransferInitialData GetInitialData(GameData game, int forPlayer)
		{
			return new TransferInitialData
			{
				Timeout = game.Configuration.TurnTimeout,
				YourPlayerId = forPlayer,
				YourUnits = game.Players[forPlayer].Units.Select(a => new TransferInitialData.YourUnitsData
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
				VisibleMap = GetVisibleMap(game, forPlayer, true),
				OtherPlayers = game.Players.Where(a => a.Key != forPlayer).Select(a => new TransferInitialData.OtherPlayerData
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

		private TransferTurnData GetTurnData(GameData game, int forPlayer, Dictionary<long, ServerTurnDelta> UnitsTurnDelta)
		{
			var player = game.Players[forPlayer];
			return new TransferTurnData
			{
				GameOverLoose = player.IsGameOver,
				GameOverWin = game.Players.Where(a => a.Key != forPlayer).All(a => a.Value.IsGameOver),
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
				VisibleMap = this.GetVisibleMap(game, forPlayer, false),
				OtherPlayers = game.Players.Concat(game.PlayersGameOver).Where(a => a.Key != forPlayer).ToDictionary(a => a.Key, a => new TransferTurnData.OtherPlayersData
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

		private MapTile[,] GetVisibleMap(GameData game, int forPlayer, bool isInitialize)
		{
			var player = game.Players[forPlayer];

			var result = new MapTile[game.Map.Paths.GetLength(0), game.Map.Paths.GetLength(1)];
			for (var x = 0; x < game.Map.Paths.GetLength(0); x++)
				for (var y = 0; y < game.Map.Paths.GetLength(1); y++)
				{
					if (!IsVisible(player, x, y) && (!game.Configuration.FullMapVisible || !isInitialize))
					{
						result[x, y] = MapTile.Unknown;
					}
					else
					{
						switch (game.Map.Paths[x, y])
						{
							case MapRepository.JunctionTileId:
							case MapRepository.MazeTileId:
							case MapRepository.RoomTileId:
								{
									result[x, y] = MapTile.Path;
									break;
								}
							case MapRepository.WallTileId:
								{
									result[x, y] = MapTile.Wall;
									break;
								}
							case MapRepository.EmptyTileId:
								{
									result[x, y] = MapTile.Unknown;
									break;
								}
						}
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

		public void CheckTimeout(GameData game, float delta)
		{
			if (!game.Configuration.TurnTimeout.HasValue)
			{
				return;
			}

			game.Timeout = game.Timeout ?? game.Configuration.TurnTimeout;
			game.Timeout -= delta;
			if (game.Timeout > 0)
			{
				return;
			}

			game.Timeout = game.Configuration.TurnTimeout;
			var forcePlayerMove = game.Players.Keys.Where(player => !game.PlayersMove.ContainsKey(player)).ToList();
			foreach (var player in forcePlayerMove)
			{
				this.PlayerMove(game, player, emptyMoves);
			}
		}
	}
}
