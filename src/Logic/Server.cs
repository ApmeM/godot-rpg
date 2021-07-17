using BrainAI.Pathfinding.BreadthFirst;
using FateRandom;
using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using IsometricGame.Logic.ScriptHelpers;
using MazeGenerators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IsometricGame.Logic
{
	public class Server
	{
		private RoomMazeGenerator.Result Map;
		private VectorGridGraph Astar;
		private readonly Dictionary<int, ServerPlayer> Players = new Dictionary<int, ServerPlayer>();
		private readonly Dictionary<int, TransferTurnDoneData> PlayersMove = new Dictionary<int, TransferTurnDoneData>();

		private ServerConfiguration configuration;
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

		public void Connect(int playerId, TransferConnectData connect, Action<TransferInitialData> initialize, Action<TransferTurnData> turnDone)
		{
            if (this.Players.ContainsKey(playerId))
            {
				throw new Exception($"Player with id {playerId} already connected");
            }

			var player = new ServerPlayer
			{
				PlayerName = connect.TeamName,
			};

			var playerNumber = Players.Count();
			var centerX = Map.Rooms[playerNumber].X + Map.Rooms[playerNumber].Width / 2;
			var centerY = this.Map.Rooms[playerNumber].Y + this.Map.Rooms[playerNumber].Height / 2;
			var center = new Vector2(centerX, centerY);
			foreach (var u in connect.Units.Take(configuration.MaxUnits))
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
			}
		}

		public void PlayerMove(int forPlayer, TransferTurnDoneData moves)
        {
            // Put player move into dictionary
            PlayersMove[forPlayer] = moves;

            if (PlayersMove.Count != Players.Count)
            {
                return;
            }

			// When all players send their turns - apply all.
			var UnitsTurnDelta = new Dictionary<long, ServerTurnDelta>();
			var occupiedCells = new HashSet<Vector2>();
			foreach (var actionPlayer in Players)
            {
                foreach (var actionUnit in actionPlayer.Value.Units)
                {
                    var fullId = UnitUtils.GetFullUnitId(actionPlayer.Key, actionUnit.Key);
					UnitsTurnDelta[fullId] = new ServerTurnDelta();

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

			foreach (var playerMove in PlayersMove)
            {
                foreach (var unitMove in playerMove.Value.UnitActions)
                {
                    var actionUnit = Players[playerMove.Key].Units[unitMove.Key];
                    if (!unitMove.Value.Move.HasValue || actionUnit.Hp <= 0)
                    {
                        continue;
                    }

                    var path = BreadthFirstPathfinder.Search(this.Astar, actionUnit.Position, unitMove.Value.Move.Value)
                        .Take(actionUnit.MoveDistance + 1)
                        .ToList();

                    for (var i = path.Count; i > 0; i--)
                    {
                        if (occupiedCells.Contains(path[i - 1]))
                        {
                            continue;
                        }

                        actionUnit.Position = path[i - 1];
                        occupiedCells.Add(actionUnit.Position);
                        break;
                    }
                }
            }

            foreach (var playerMove in PlayersMove)
            {
				foreach (var unitMove in playerMove.Value.UnitActions)
				{
					var actionFullId = UnitUtils.GetFullUnitId(playerMove.Key, unitMove.Key);
					var actionDelta = UnitsTurnDelta[actionFullId];
					var actionPlayer = Players[playerMove.Key];
					var actionUnit = actionPlayer.Units[unitMove.Key];
					if (unitMove.Value.AbilityDirection.HasValue && actionUnit.Hp > 0)
					{
						if (!actionUnit.Abilities.Contains(unitMove.Value.Ability))
						{
							continue;
						}

						var ability = UnitUtils.FindAbility(unitMove.Value.Ability);

						if (ability.TargetUnit)
						{
							var targetPlayerId = UnitUtils.GetPlayerId(unitMove.Value.AbilityFullUnitId);
							var targetUnitId = UnitUtils.GetUnitId(unitMove.Value.AbilityFullUnitId);
							var targetPlayer = Players[targetPlayerId];
							var targetUnit = targetPlayer.Units[targetUnitId];

							actionDelta.AbilityDirection = targetUnit.Position - actionUnit.Position;
						}
						else
						{
							actionDelta.AbilityDirection = unitMove.Value.AbilityDirection.Value;
						}

						foreach (var targetPlayer in Players)
						{
							foreach (var targetUnit in targetPlayer.Value.Units)
							{
								if (ability.IsApplicable(this.Astar, actionPlayer, actionUnit, targetPlayer.Value, targetUnit.Value, actionDelta.AbilityDirection.Value))
								{
									var targetFullId = UnitUtils.GetFullUnitId(targetPlayer.Key, targetUnit.Key);
									var targetDelta = UnitsTurnDelta[targetFullId];
									targetDelta.AbilityFrom = actionUnit.Position - targetUnit.Value.Position;
									targetDelta.Actions.AddRange(ability.Apply(actionUnit, targetUnit.Value));
								}
							}
						}
					}
				}
            }

            PlayersMove.Clear();

			foreach (var p in Players)
			{
				foreach (var u in p.Value.Units)
				{
					var targetFullId = UnitUtils.GetFullUnitId(p.Key, u.Key);
					var targetDelta = UnitsTurnDelta[targetFullId];
					foreach (var action in targetDelta.Actions)
					{
						action.Apply(u.Value);
					}
					
					UnitUtils.RefreshUnit(p.Value, u.Value);

					u.Value.Hp = Math.Max(0, u.Value.Hp);
				}
			}

            foreach (var turnDoneMethod in turnDoneMethods)
            {
                turnDoneMethod.Value(GetTurnData(turnDoneMethod.Key, UnitsTurnDelta));
            }
        }

		private TransferInitialData GetInitialData(int forPlayer)
		{
			return new TransferInitialData
			{
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
				VisibleMap = GetVisibleMap(forPlayer),
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
				YourUnits = player.Units.ToDictionary(a => a.Key, a =>
				{
					var fullId = UnitUtils.GetFullUnitId(forPlayer, a.Key);
					var delta = UnitsTurnDelta[fullId];
					return new TransferTurnData.YourUnitsData
					{
						Position = a.Value.Position,
						AttackDirection = a.Value.Hp <= 0 ? null : delta.AbilityDirection,
						Hp = a.Value.Hp,
						AttackFrom = a.Value.Hp <= 0 ? null : delta.AbilityFrom,
						Effects = a.Value.Effects,
						MoveDistance = a.Value.MoveDistance,
						SightRange = a.Value.SightRange,
						RangedAttackDistance = a.Value.RangedAttackDistance,
						AOEAttackRadius = a.Value.AOEAttackRadius,
						AttackPower = a.Value.AttackPower,
						MagicPower = a.Value.MagicPower,
					};
				}),
				VisibleMap = this.GetVisibleMap(forPlayer),
				OtherPlayers = this.Players.Where(a => a.Key != forPlayer).ToDictionary(a => a.Key, a => new TransferTurnData.OtherPlayersData
				{
					Units = a.Value.Units.Where(b => IsVisible(player, (int)b.Value.Position.x, (int)b.Value.Position.y)).ToDictionary(b => b.Key, b =>
					{
						var fullId = UnitUtils.GetFullUnitId(a.Key, b.Key);
						var delta = UnitsTurnDelta[fullId];

						return new TransferTurnData.OtherUnitsData
						{
							Position = b.Value.Position,
							AttackDirection = b.Value.Hp <= 0 ? null : delta.AbilityDirection,
							Hp = b.Value.Hp,
							AttackFrom = b.Value.Hp <= 0 ? null : delta.AbilityFrom,
							Effects = b.Value.Effects,
						};
					})
				})
			};
		}

		private MapTile[,] GetVisibleMap(int forPlayer)
		{
			var player = Players[forPlayer];

			var result = new MapTile[Map.Regions.GetLength(0), Map.Regions.GetLength(1)];
			for (var x = 0; x < Map.Regions.GetLength(0); x++)
				for (var y = 0; y < Map.Regions.GetLength(1); y++)
				{
					if (!IsVisible(player, x, y) && !configuration.FullMapVisible)
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
	}
}
