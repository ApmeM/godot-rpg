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
		private Dictionary<int, ServerPlayer> Players = new Dictionary<int, ServerPlayer>();

		public void Start()
		{
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

			Connect("Bot1");
		}

		public int Connect(string playerName)
		{
			var playerId = Players.Count + 1;
			var player = new ServerPlayer
			{
				PlayerName = playerName,
			};

			var centerX = Map.Rooms[playerId].X + Map.Rooms[playerId].Width / 2;
			var centerY = this.Map.Rooms[playerId].Y + this.Map.Rooms[playerId].Height / 2;

			player.Units.Add(1, new ServerUnit { Position = new Vector2(centerX - 1, centerY), MoveDistance = Fate.GlobalFate.Range(3, 6), SightRange = Fate.GlobalFate.Range(4, 7) });
			player.Units.Add(2, new ServerUnit { Position = new Vector2(centerX + 1, centerY), MoveDistance = Fate.GlobalFate.Range(3, 6), SightRange = Fate.GlobalFate.Range(4, 7) });
			player.Units.Add(3, new ServerUnit { Position = new Vector2(centerX, centerY + 1), MoveDistance = Fate.GlobalFate.Range(3, 6), SightRange = Fate.GlobalFate.Range(4, 7) });
			player.Units.Add(4, new ServerUnit { Position = new Vector2(centerX, centerY - 1), MoveDistance = Fate.GlobalFate.Range(3, 6), SightRange = Fate.GlobalFate.Range(4, 7) });

			this.Players.Add(playerId, player);

			return playerId;
		}

		public TransferInitialData GetInitialData(int forPlayer)
		{
			return new TransferInitialData
			{
				YourUnits = Players[forPlayer].Units.Select(a => new TransferInitialUnit
				{
					UnitId = a.Key,
					Position = a.Value.Position,
					MoveDistance = a.Value.MoveDistance,
					SightRange = a.Value.SightRange
				}).ToList(),
				VisibleMap = GetVisibleMap(forPlayer),
				OtherPlayers = Players.Where(a => a.Key != forPlayer).Select(a => new TransferInitialPlayer
				{
					PlayerId = a.Key,
					PlayerName = a.Value.PlayerName,
					Units = a.Value.Units.Keys.ToList()
				}).ToList()
			};
		}

		public void PlayerMove(int forPlayer, Dictionary<int, Vector2> moves)
		{
			var player = Players[forPlayer];
			ApplyMoves(player, moves);

			var otherPlayers = Players.Where(a => a.Key != forPlayer).Select(a => a.Value).ToList();
			foreach(var p in otherPlayers)
			{
				var otherMoves = new Dictionary<int, Vector2>();
				foreach(var u in p.Units)
				{
					otherMoves.Add(u.Key, u.Value.Position + Fate.GlobalFate.Choose(Vector2.Up, Vector2.Down, Vector2.Left, Vector2.Right));
				}

				ApplyMoves(p, otherMoves);
			}
		}

		private void ApplyMoves(ServerPlayer player, Dictionary<int, Vector2> moves)
		{
			foreach (var move in moves)
			{
				var forUnit = move.Key;
				var newTarget = move.Value;

				var unit = player.Units[forUnit];

				BreadthFirstPathfinder.Search(this.Astar, unit.Position, unit.MoveDistance, out var result);

				if (result.ContainsKey(newTarget))
				{
					unit.Position = newTarget;
				}
			}
		}

		public TransferTurnData GetTurnData(int forPlayer)
		{
			var player = Players[forPlayer];
			return new TransferTurnData
			{
				YourUnits = player.Units.ToDictionary(a => a.Key, a => new TransferTurnUnit
				{
					Position = a.Value.Position
				}),
				VisibleMap = this.GetVisibleMap(forPlayer),
				OtherPlayers = this.Players.Where(a => a.Key != forPlayer).ToDictionary(a => a.Key, a => new TransferTurnPlayer
				{
					Units = a.Value.Units.Where(b => IsVisible(player, (int)b.Value.Position.x, (int)b.Value.Position.y)).ToDictionary(b => b.Key, b => new TransferTurnPlayerUnit
					{
						Position = b.Value.Position
					})
				})
			};
		}

		public MapTile[,] GetVisibleMap(int forPlayer)
		{
			var player = Players[forPlayer];

			var result = new MapTile[Map.Regions.GetLength(0), Map.Regions.GetLength(1)];
			for (var x = 0; x < Map.Regions.GetLength(0); x++)
				for (var y = 0; y < Map.Regions.GetLength(1); y++)
				{
					if (!IsVisible(player, x, y))
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
	}
}
