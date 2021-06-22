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
		private readonly Dictionary<long, ServerTurnDelta> UnitsTurnDelta = new Dictionary<long, ServerTurnDelta>();

		private ServerConfiguration configuration;
		private readonly Dictionary<int, Action<TransferInitialData>> initializeMethods = new Dictionary<int, Action<TransferInitialData>>();
		private readonly Dictionary<int, Action<TransferTurnData>> turnDoneMethods = new Dictionary<int, Action<TransferTurnData>>();
		
		public void Start(ServerConfiguration configuration)
		{
			this.configuration = configuration;
			var generator = new RoomMazeGenerator();
			this.Map = generator.Generate(new RoomMazeGenerator.Settings
			{
				Width = 21,
				Height = 21,
				MinRoomSize = 5,
				MaxRoomSize = 9,
			});

			this.Astar = new VectorGridGraph(Map.Regions.GetLength(0), Map.Regions.GetLength(1));
			for (var x = 0; x < Map.Regions.GetLength(0); x++)
				for (var y = 0; y < Map.Regions.GetLength(1); y++)
				{
					if (!Map.Regions[x, y].HasValue)
					{
						this.Astar.Walls.Add(new Vector2(x, y));
					}
				}
		}

		public void Connect(TransferConnectData connect, Action<TransferInitialData> initialize, Action<TransferTurnData> turnDone)
		{
			var playerId = Players.Count + 1;
			var player = new ServerPlayer
			{
				PlayerName = connect.PlayerName,
			};

			var centerX = Map.Rooms[playerId].X + Map.Rooms[playerId].Width / 2;
			var centerY = this.Map.Rooms[playerId].Y + this.Map.Rooms[playerId].Height / 2;

			var unitId = 0;
			foreach(var u in connect.Units)
			{
				unitId++;
				if(unitId > configuration.MaxUnits)
				{
					break;
				}

				var unit = UnitUtils.BuildUnit(u.UnitType);
				player.Units.Add(unitId, unit);
				for (var i = 0; i < u.Skills.Count; i++)
				{
					if (i == configuration.MaxSkills)
					{
						break;
					}
					var skill = u.Skills[i];
					UnitUtils.ApplySkill(player, unit, skill);
				}
			}

			player.Units[1].Position = new Vector2(centerX - 1, centerY);
			player.Units[2].Position = new Vector2(centerX + 1, centerY);
			player.Units[3].Position = new Vector2(centerX, centerY + 1);
			player.Units[4].Position = new Vector2(centerX, centerY - 1);

			this.Players.Add(playerId, player);

			this.initializeMethods[playerId] = initialize;
			this.turnDoneMethods[playerId] = turnDone;

			if (playerId == configuration.PlayersCount)
			{
				foreach(var initMethod in initializeMethods)
				{
					initMethod.Value(GetInitialData(initMethod.Key));
				}
			}
		}

		public void PlayerMove(int forPlayer, TransferTurnDoneData moves)
		{
			// Put player move into dictionary
			PlayersMove[forPlayer] = moves;

			// When all players send their turns - apply all.
			if (PlayersMove.Count == Players.Count)
			{
				this.UnitsTurnDelta.Clear();
				foreach (var p in Players)
				{
					foreach (var u in p.Value.Units)
					{
						var fullId = GetFullUnitId(p.Key, u.Key);
						UnitsTurnDelta[fullId] = new ServerTurnDelta
						{
							MovedFrom = u.Value.Position,
							MovedTo = u.Value.Position
						};
					}
				}

				foreach (var pm in PlayersMove)
				{
					foreach (var um in pm.Value.UnitActions)
					{
						var fullId = GetFullUnitId(pm.Key, um.Key);
						var delta = UnitsTurnDelta[fullId];
						var unit = Players[pm.Key].Units[um.Key];
						if (um.Value.Move.HasValue && unit.Hp > 0)
						{
							BreadthFirstPathfinder.Search(this.Astar, unit.Position, unit.MoveDistance, out var result);

							if (result.ContainsKey(um.Value.Move.Value))
							{
								delta.MovedTo = um.Value.Move.Value;
								unit.Position = um.Value.Move.Value;
							}
						}
					}
				}

				foreach (var pm in PlayersMove)
				{
					foreach (var um in pm.Value.UnitActions)
					{
						var fullId = GetFullUnitId(pm.Key, um.Key);
						var delta = UnitsTurnDelta[fullId];
						var player = Players[pm.Key];
						var unit = player.Units[um.Key];
						if (um.Value.UsableTarget.HasValue && unit.Hp > 0)
						{
							delta.UsableDirection = um.Value.UsableTarget.Value;
							delta.Usable = um.Value.Usable;
							if (!unit.Usables.Contains(delta.Usable))
							{
								continue;
							}
							var usable = UnitUtils.FindUsable(delta.Usable);
							foreach (var p in Players)
							{
								foreach (var u in p.Value.Units)
								{
									if (!usable.IsApplicable(player, unit, p.Value, u.Value))
									{
										continue;
									}

									if (usable.IsInRange(unit, u.Value, delta.UsableDirection.Value))
									{
										var fullIdTarget = GetFullUnitId(p.Key, u.Key);
										var deltaTarget = UnitsTurnDelta[fullIdTarget];
										deltaTarget.UsableFrom = unit.Position - u.Value.Position;
										usable.Apply(unit, u.Value);
									}
								}
							}
						}
					}
				}

				PlayersMove.Clear();

				foreach (var turnDoneMethod in turnDoneMethods)
				{
					turnDoneMethod.Value(GetTurnData(turnDoneMethod.Key));
				}
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
					SightRange = a.Value.VisionRange,
					AttackDistance = a.Value.AttackDistance,
					AttackRadius = a.Value.AttackRadius,
					AttackPower = a.Value.AttackDamage,
					MaxHp = a.Value.MaxHp,
					UnitType = a.Value.UnitType,
					Usables = a.Value.Usables.ToList()
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
						MaxHp = b.Value.MaxHp
					}).ToList(),
				}).ToList()
			};
		}

		private TransferTurnData GetTurnData(int forPlayer)
		{
			var player = Players[forPlayer];
			return new TransferTurnData
			{
				YourUnits = player.Units.ToDictionary(a => a.Key, a =>
				{
					var fullId = GetFullUnitId(forPlayer, a.Key);
					var delta = UnitsTurnDelta[fullId];
					return new TransferTurnData.YourUnitsData
					{
						Position = delta.MovedTo,
						AttackDirection = delta.UsableDirection,
						Hp = a.Value.Hp,
						AttackFrom = delta.UsableFrom,
					};
				}),
				VisibleMap = this.GetVisibleMap(forPlayer),
				OtherPlayers = this.Players.Where(a => a.Key != forPlayer).ToDictionary(a => a.Key, a => new TransferTurnData.OtherPlayersData
				{
					Units = a.Value.Units.Where(b => IsVisible(player, (int)b.Value.Position.x, (int)b.Value.Position.y)).ToDictionary(b => b.Key, b =>
					{
						var fullId = GetFullUnitId(a.Key, b.Key);
						var delta = UnitsTurnDelta[fullId];

						return new TransferTurnData.OtherUnitsData
						{
							Position = delta.MovedTo,
							AttackDirection = delta.UsableDirection,
							Hp = b.Value.Hp,
							AttackFrom = delta.UsableFrom,
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
				if ((Math.Abs(x - unit.Position.x) + Math.Abs(y - unit.Position.y)) <= unit.VisionRange){
					return true;
				}
			}

			return false;
		}

		private static long GetFullUnitId(int playerId, int unitId)
		{
			return ((long)playerId << 32) | ((long)unitId & 0xFFFFFFFFL);
		}
	}
}
