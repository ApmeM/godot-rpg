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

		private RoomMazeGenerator.Result Map;
		private VectorGridGraph Astar;
		private readonly Dictionary<int, ServerPlayer> Players = new Dictionary<int, ServerPlayer>();
		private readonly Dictionary<int, ServerPlayer> PlayersGameOver = new Dictionary<int, ServerPlayer>();
		private readonly Dictionary<int, TransferTurnDoneData> PlayersMove = new Dictionary<int, TransferTurnDoneData>();

		private ServerConfiguration configuration;
        private float? Timeout;
        private readonly Dictionary<int, Action<TransferInitialData>> initializeMethods = new Dictionary<int, Action<TransferInitialData>>();
		private readonly Dictionary<int, Action<TransferTurnData>> turnDoneMethods = new Dictionary<int, Action<TransferTurnData>>();
		
		public void Start(ServerConfiguration configuration)
		{
			this.configuration = configuration;
			var generator = new RoomMazeGenerator();
			this.Map = generator.Generate(new RoomMazeGenerator.Settings
			{
				Width = configuration.PlayersCount * 10 + 1,
				Height = configuration.PlayersCount * 10 + 1,
				MinRoomSize = 5,
				MaxRoomSize = 9,
			});

			this.Astar = new VectorGridGraph(Map.Regions.GetLength(0), Map.Regions.GetLength(1));
			for (var x = 0; x < Map.Regions.GetLength(0); x++)
				for (var y = 0; y < Map.Regions.GetLength(1); y++)
				{
					if (Map.Regions[x, y].HasValue)
					{
						this.Astar.Paths.Add(new Vector2(x, y));
					}
				}
		}

		public void Connect(int playerId, TransferConnectData connectData, 
			Action<TransferInitialData> initialize, 
			Action<TransferTurnData> turnDone)
		{
            if (this.Players.ContainsKey(playerId))
            {
				throw new Exception($"Player with id {playerId} already connected");
            }

			var player = new ServerPlayer
			{
				PlayerName = connectData.TeamName,
			};

			var playerNumber = Players.Count();
			var centerX = Map.Rooms[playerNumber].X + Map.Rooms[playerNumber].Width / 2;
			var centerY = this.Map.Rooms[playerNumber].Y + this.Map.Rooms[playerNumber].Height / 2;
			var center = new Vector2(centerX, centerY);
			foreach (var u in connectData.Units.Take(configuration.MaxUnits))
			{
				player.Units.Add(player.Units.Count + 1, UnitUtils.BuildUnit(player, u.UnitType, u.Skills.Take(configuration.MaxSkills).ToList()));
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

			this.Players.Add(playerId, player);

			this.initializeMethods[playerId] = initialize;
			this.turnDoneMethods[playerId] = turnDone;

			if (playerNumber + 1 == configuration.PlayersCount)
			{
				foreach (var p in Players)
				{
					foreach (var u in p.Value.Units)
					{
						UnitUtils.RefreshUnit(p.Value, u.Value);
					}
				}

				foreach (var initMethod in initializeMethods)
				{
					initMethod.Value(GetInitialData(initMethod.Key));
				}

				foreach (var turnDoneMethod in turnDoneMethods)
				{
					turnDoneMethod.Value(GetTurnData(turnDoneMethod.Key, new Dictionary<long, ServerTurnDelta>()));
				}
			}
		}

		public void PlayerMove(int forPlayer, TransferTurnDoneData moves)
        {
            if (PlayersGameOver.ContainsKey(forPlayer))
            {
				return;
            }

            // Put player move into dictionary
            PlayersMove[forPlayer] = moves;

            if (PlayersMove.Count != Players.Count)
            {
                return;
            }

			// When all players send their turns - apply all.
			this.Timeout = configuration.TurnTimeout;
			var unitsTurnDelta = new Dictionary<long, ServerTurnDelta>();
			var occupiedCells = new HashSet<Vector2>();
			var appliedActions = new List<IAppliedAction>();

			/* Initialize turn delta. */
			foreach (var actionPlayer in Players)
			{
				foreach (var actionUnit in actionPlayer.Value.Units)
				{
					var fullId = UnitUtils.GetFullUnitId(actionPlayer.Key, actionUnit.Key);
					unitsTurnDelta[fullId] = new ServerTurnDelta();
				}
			}

			/* Calculate move blockers */
			foreach (var actionPlayer in Players)
            {
                foreach (var actionUnit in actionPlayer.Value.Units)
                {
					if (actionUnit.Value.Hp <= 0)
                    {
						occupiedCells.Add(actionUnit.Value.Position);
						continue;
					}
					if (!PlayersMove.ContainsKey(actionPlayer.Key))
                    {
						occupiedCells.Add(actionUnit.Value.Position);
						continue;
                    }
					var playerMove = PlayersMove[actionPlayer.Key];
                    if (!playerMove.UnitActions.ContainsKey(actionUnit.Key))
                    {
						occupiedCells.Add(actionUnit.Value.Position);
						continue;
					}
					var unitMove = playerMove.UnitActions[actionUnit.Key];
					if (!unitMove.Move.HasValue)
                    {
						occupiedCells.Add(actionUnit.Value.Position);
						continue;
					}
				}
			}

			/* Move units */
			foreach (var actionPlayer in Players)
			{
				foreach (var actionUnit in actionPlayer.Value.Units)
				{
					if (!PlayersMove.ContainsKey(actionPlayer.Key))
                    {
						continue;
                    }

					var playerMove = PlayersMove[actionPlayer.Key];
                    if (!playerMove.UnitActions.ContainsKey(actionUnit.Key))
                    {
						continue;
                    }
					
					var unitMove = playerMove.UnitActions[actionUnit.Key];
                 
					if (!unitMove.Move.HasValue || actionUnit.Value.Hp <= 0)
                    {
                        continue;
                    }

                    var path = BreadthFirstPathfinder.Search(this.Astar, actionUnit.Value.Position, unitMove.Move.Value)
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
			foreach (var actionPlayer in Players)
			{
				foreach (var actionUnit in actionPlayer.Value.Units)
				{
					if (!PlayersMove.ContainsKey(actionPlayer.Key))
					{
						continue;
					}

					var playerMove = PlayersMove[actionPlayer.Key];
					if (!playerMove.UnitActions.ContainsKey(actionUnit.Key))
					{
						continue;
					}

					var unitMove = playerMove.UnitActions[actionUnit.Key];
					var actionFullId = UnitUtils.GetFullUnitId(actionPlayer.Key, actionUnit.Key);
					var actionDelta = unitsTurnDelta[actionFullId];
					if (unitMove.AbilityDirection.HasValue && actionUnit.Value.Hp > 0)
					{
						if (!actionUnit.Value.Abilities.Contains(unitMove.Ability))
						{
							continue;
						}

						var ability = UnitUtils.FindAbility(unitMove.Ability);

						if (ability.TargetUnit)
						{
							var targetPlayerId = UnitUtils.GetPlayerId(unitMove.AbilityFullUnitId);
							var targetUnitId = UnitUtils.GetUnitId(unitMove.AbilityFullUnitId);
							var targetPlayer = Players[targetPlayerId];
							var targetUnit = targetPlayer.Units[targetUnitId];

							actionDelta.AbilityDirection = targetUnit.Position - actionUnit.Value.Position;
						}
						else
						{
							actionDelta.AbilityDirection = unitMove.AbilityDirection.Value;
						}

						foreach (var targetPlayer in Players)
						{
							foreach (var targetUnit in targetPlayer.Value.Units)
							{
								if (ability.IsApplicable(this.Astar, actionPlayer.Value, actionUnit.Value, targetPlayer.Value, targetUnit.Value, actionDelta.AbilityDirection.Value))
								{
									var targetFullId = UnitUtils.GetFullUnitId(targetPlayer.Key, targetUnit.Key);
									var targetDelta = unitsTurnDelta[targetFullId];
									targetDelta.AbilityFrom = actionUnit.Value.Position - targetUnit.Value.Position;

                                    var actions = ability.Apply(actionUnit.Value, targetUnit.Value);
									appliedActions.AddRange(actions);
									targetDelta.HpChanges.AddRange(actions.OfType<ChangeHpAppliedAction>().Select(a => a.Value).ToList());
								}
							}
						}
					}
				}
            }

			foreach (var action in appliedActions)
			{
				action.Apply();
			}
			appliedActions.Clear();

			/* Refresh unit values. */
			foreach (var actionPlayer in Players)
			{
				foreach (var actionUnit in actionPlayer.Value.Units)
				{
					UnitUtils.RefreshUnit(actionPlayer.Value, actionUnit.Value);
				}
			}

			/* Calculate all effects */
			foreach (var actionPlayer in Players)
			{
				foreach (var actionUnit in actionPlayer.Value.Units)
				{
					var targetFullId = UnitUtils.GetFullUnitId(actionPlayer.Key, actionUnit.Key);
					var targetDelta = unitsTurnDelta[targetFullId];

					foreach (var effect in actionUnit.Value.Effects)
					{
						var actions = UnitUtils.FindEffect(effect.Effect).Apply(actionUnit.Value);
						appliedActions.AddRange(actions);
						targetDelta.HpChanges.AddRange(actions.OfType<ChangeHpAppliedAction>().Select(a => a.Value).ToList());
						effect.Duration--;
					}

					actionUnit.Value.Effects.RemoveAll(a => a.Duration <= 0);
				}
			}

			foreach (var action in appliedActions)
			{
				action.Apply();
			}
			appliedActions.Clear();

			/* Check survived units */
			foreach (var actionPlayer in Players)
			{
				foreach (var actionUnit in actionPlayer.Value.Units)
				{
					actionUnit.Value.Hp = Mathf.Clamp(actionUnit.Value.Hp, 0, actionUnit.Value.MaxHp);
				}

				actionPlayer.Value.IsGameOver = actionPlayer.Value.IsGameOver || CheckGameOver(actionPlayer.Value);
			}

			PlayersMove.Clear();

			foreach (var turnDoneMethod in turnDoneMethods)
			{
				turnDoneMethod.Value(GetTurnData(turnDoneMethod.Key, unitsTurnDelta));
			}
			
			foreach (var actionPlayer in Players.ToList())
			{
                if (!actionPlayer.Value.IsGameOver)
                {
					continue;
                }
				turnDoneMethods.Remove(actionPlayer.Key);
				initializeMethods.Remove(actionPlayer.Key);
				Players.Remove(actionPlayer.Key);
				PlayersGameOver[actionPlayer.Key] = actionPlayer.Value;
			}
			
			// Todo: Remove lobby if game is over for all players.
		}

        private TransferInitialData GetInitialData(int forPlayer)
		{
			return new TransferInitialData
			{
				Timeout = this.configuration.TurnTimeout,
				YourPlayerId = forPlayer,
				YourUnits = Players[forPlayer].Units.Select(a => new TransferInitialData.YourUnitsData
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
					UnitType = a.Value.UnitType,
					Abilities = a.Value.Abilities.ToList(),
					Skills = a.Value.Skills.ToList()
				}).ToList(),
				VisibleMap = GetVisibleMap(forPlayer, true),
				OtherPlayers = Players.Where(a => a.Key != forPlayer).Select(a => new TransferInitialData.OtherPlayerData
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

		private TransferTurnData GetTurnData(int forPlayer, Dictionary<long, ServerTurnDelta> UnitsTurnDelta)
		{
			var player = Players[forPlayer];
			return new TransferTurnData
			{
				GameOverLoose = player.IsGameOver,
				GameOverWin = Players.Where(a => a.Key != forPlayer).All(a => a.Value.IsGameOver),
				YourUnits = player.Units.ToDictionary(a => a.Key, a =>
				{
					var fullId = UnitUtils.GetFullUnitId(forPlayer, a.Key);
					UnitsTurnDelta.TryGetValue(fullId, out var delta);
					return new TransferTurnData.YourUnitsData
					{
						Position = a.Value.Position,
						AttackDirection = a.Value.Hp <= 0 ? null : delta?.AbilityDirection,
						Hp = a.Value.Hp,
						AttackFrom = a.Value.Hp <= 0 ? null : delta?.AbilityFrom,
						Effects = a.Value.Effects,
						MoveDistance = a.Value.MoveDistance,
						SightRange = a.Value.SightRange,
						RangedAttackDistance = a.Value.RangedAttackDistance,
						AOEAttackRadius = a.Value.AOEAttackRadius,
						AttackPower = a.Value.AttackPower,
						MagicPower = a.Value.MagicPower,
						Changes = delta?.HpChanges
					};
				}),
				VisibleMap = this.GetVisibleMap(forPlayer, false),
				OtherPlayers = this.Players.Where(a => a.Key != forPlayer).ToDictionary(a => a.Key, a => new TransferTurnData.OtherPlayersData
				{
					Units = a.Value.Units.Where(b => IsVisible(player, (int)b.Value.Position.x, (int)b.Value.Position.y)).ToDictionary(b => b.Key, b =>
					{
						var fullId = UnitUtils.GetFullUnitId(a.Key, b.Key);
						UnitsTurnDelta.TryGetValue(fullId, out var delta);

						return new TransferTurnData.OtherUnitsData
						{
							Position = b.Value.Position,
							AttackDirection = b.Value.Hp <= 0 ? null : delta?.AbilityDirection,
							Hp = b.Value.Hp,
							AttackFrom = b.Value.Hp <= 0 ? null : delta?.AbilityFrom,
							Effects = b.Value.Effects,
							Changes = delta?.HpChanges
						};
					})
				})
			};
		}

        private bool CheckGameOver(ServerPlayer player)
        {
			return player.Units.All(unit => unit.Value.Hp <= 0);
        }

        private MapTile[,] GetVisibleMap(int forPlayer, bool isInitialize)
		{
			var player = Players[forPlayer];

			var result = new MapTile[Map.Regions.GetLength(0), Map.Regions.GetLength(1)];
			for (var x = 0; x < Map.Regions.GetLength(0); x++)
				for (var y = 0; y < Map.Regions.GetLength(1); y++)
				{
					if (!IsVisible(player, x, y) && (!configuration.FullMapVisible || !isInitialize ))
					{
						result[x, y] = MapTile.Unknown;
					}
					else if (Map.Regions[x, y].HasValue)
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
			foreach(var unit in player.Units.Values)
			{
				if (unit.Hp <= 0)
				{
					continue;
				}

				if ((Math.Abs(x - unit.Position.x) + Math.Abs(y - unit.Position.y)) <= unit.SightRange){
					return true;
				}
			}

			return false;
		}

		public void CheckTimeout(float delta)
        {
            if (!configuration.TurnTimeout.HasValue)
            {
                return;
            }
            
			this.Timeout = this.Timeout ?? configuration.TurnTimeout;
            this.Timeout -= delta;
            if (this.Timeout > 0)
            {
				return;
            }

			this.Timeout = configuration.TurnTimeout;
			foreach(var player in this.Players)
            {
                if (PlayersMove.ContainsKey(player.Key))
                {
					continue;
                }

                this.PlayerMove(player.Key, emptyMoves);
            }
        }
    }
}
