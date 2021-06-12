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

		public void Connect(string playerName, Action<TransferInitialData> initialize, Action<TransferTurnData> turnDone)
		{
			var playerId = Players.Count + 1;
			var player = new ServerPlayer
			{
				PlayerName = playerName,
			};

			var centerX = Map.Rooms[playerId].X + Map.Rooms[playerId].Width / 2;
			var centerY = this.Map.Rooms[playerId].Y + this.Map.Rooms[playerId].Height / 2;

			player.Units.Add(1, new ServerUnit { Position = new Vector2(centerX - 1, centerY), MoveDistance = Fate.GlobalFate.Range(4, 6), SightRange = Fate.GlobalFate.Range(6, 7) });
			player.Units.Add(2, new ServerUnit { Position = new Vector2(centerX + 1, centerY), MoveDistance = Fate.GlobalFate.Range(4, 6), SightRange = Fate.GlobalFate.Range(6, 7) });
			player.Units.Add(3, new ServerUnit { Position = new Vector2(centerX, centerY + 1), MoveDistance = Fate.GlobalFate.Range(4, 6), SightRange = Fate.GlobalFate.Range(6, 7) });
			player.Units.Add(4, new ServerUnit { Position = new Vector2(centerX, centerY - 1), MoveDistance = Fate.GlobalFate.Range(4, 6), SightRange = Fate.GlobalFate.Range(6, 7) });

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
							MovedTo = u.Value.Position,
							HpBefore = u.Value.Hp,
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
						var unit = Players[pm.Key].Units[um.Key];
						if (um.Value.Attack.HasValue && unit.Hp > 0)
						{
							delta.AttackDirection = um.Value.Attack.Value;
							foreach (var p in Players)
							{
								if (!unit.AttackFriendlyFire && p.Key == pm.Key)
								{
									continue;
								}

								foreach (var u in p.Value.Units)
								{
									if (IsometricMove.Distance(u.Value.Position, unit.Position + delta.AttackDirection.Value) <= unit.AttackRadius && u.Value.Hp > 0)
									{
										var fullIdTarget = GetFullUnitId(p.Key, u.Key);
										var deltaTarget = UnitsTurnDelta[fullIdTarget];
										deltaTarget.HpChanges = (delta.HpChanges ?? 0) + unit.AttackPower;
										deltaTarget.AttackFrom = unit.Position - u.Value.Position;
										u.Value.Hp -= unit.AttackPower;
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
				YourUnits = Players[forPlayer].Units.Select(a => new TransferInitialUnit
				{
					UnitId = a.Key,
					Position = a.Value.Position,
					MoveDistance = a.Value.MoveDistance,
					SightRange = a.Value.SightRange,
					AttackDistance = a.Value.AttackDistance,
					AttackRadius = a.Value.AttackRadius,
					AttackPower = a.Value.AttackPower,
					Hp = a.Value.Hp,
				}).ToList(),
				VisibleMap = GetVisibleMap(forPlayer),
				OtherPlayers = Players.Where(a => a.Key != forPlayer).Select(a => new TransferInitialPlayer
				{
					PlayerId = a.Key,
					PlayerName = a.Value.PlayerName,
					Units = a.Value.Units.Keys.ToList(),
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
					return new TransferTurnUnit
					{
						Position = delta.MovedTo,
						AttackDirection = delta.AttackDirection,
						HpReduced = delta.HpChanges,
						AttackFrom = delta.AttackFrom,
						IsDead = a.Value.Hp <= 0
					};
				}),
				VisibleMap = this.GetVisibleMap(forPlayer),
				OtherPlayers = this.Players.Where(a => a.Key != forPlayer).ToDictionary(a => a.Key, a => new TransferTurnPlayer
				{
					Units = a.Value.Units.Where(b => IsVisible(player, (int)b.Value.Position.x, (int)b.Value.Position.y)).ToDictionary(b => b.Key, b =>
					{
						var fullId = GetFullUnitId(a.Key, b.Key);
						var delta = UnitsTurnDelta[fullId];

						return new TransferTurnPlayerUnit
						{
							Position = delta.MovedTo,
							AttackDirection = delta.AttackDirection,
							HpReduced = delta.HpChanges,
							AttackFrom = delta.AttackFrom,
							IsDead = b.Value.Hp <= 0
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
				if ((Math.Abs(x - unit.Position.x) + Math.Abs(y - unit.Position.y)) <= unit.SightRange){
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
