using BrainAI.Pathfinding;
using BrainAI.Pathfinding.AStar;
using BrainAI.Pathfinding.BreadthFirst;
using Godot;
using IsometricGame.Logic.Models;
using MazeGenerators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IsometricGame.Logic
{
	public class Server
	{
		private RoomMazeGenerator.Result Map;
		private AstarGridGraph Astar;
		private Dictionary<int, Player> Players = new Dictionary<int, Player>();

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

			this.Astar = new AstarGridGraph(Map.Regions.GetLength(0), Map.Regions.GetLength(1));
			for (var x = 0; x < Map.Regions.GetLength(0); x++)
				for (var y = 0; y < Map.Regions.GetLength(1); y++)
				{
					if (!Map.Regions[x, y].HasValue)
					{
						this.Astar.Walls.Add(new Point(x, y));
					}
				}

			Connect("Bot1");
		}

		public int Connect(string playerName)
		{
			var playerId = Players.Count;
			var player = new Player
			{
				PlayerName = playerName,
			};

			var centerX = Map.Rooms[playerId].X + Map.Rooms[playerId].Width / 2;
			var centerY = this.Map.Rooms[playerId].Y + this.Map.Rooms[playerId].Height / 2;

			player.Units.Add(1, new Unit { PlayerId = playerId, UnitId = 1, PositionX = centerX - 1, PositionY = centerY });
			player.Units.Add(2, new Unit { PlayerId = playerId, UnitId = 2, PositionX = centerX + 1, PositionY = centerY });
			player.Units.Add(3, new Unit { PlayerId = playerId, UnitId = 3, PositionX = centerX, PositionY = centerY + 1 });
			player.Units.Add(4, new Unit { PlayerId = playerId, UnitId = 4, PositionX = centerX, PositionY = centerY - 1 });

			this.Players.Add(playerId, player);

			return playerId;
		}

		public InitialData GetInitialData(int forPlayer)
		{
			return new InitialData {
				MapWidth = Map.Regions.GetLength(0),
				MapHeight = Map.Regions.GetLength(1),
				YourUnits = Players[forPlayer].Units.Values.ToList(),
				VisibleMap = GetVisibleMap(forPlayer),
				Players = Players
			};
		}

		public void PlayerMove(int forPlayer, Dictionary<int, Vector2> moves)
		{
			var player = Players[forPlayer];
			ApplyMoves(player, moves);

			GD.Print("Handling player ", player.PlayerName);

			var otherPlayers = Players.Where(a => a.Key != forPlayer).Select(a => a.Value).ToList();
			foreach(var p in otherPlayers)
			{
				var otherMoves = new Dictionary<int, Vector2>();
				foreach(var u in p.Units)
				{
					otherMoves.Add(u.Key, FateRandom.Fate.GlobalFate.Choose(Vector2.Up, Vector2.Down, Vector2.Left, Vector2.Right) + new Vector2(u.Value.PositionX, u.Value.PositionY));
				}

				ApplyMoves(p, otherMoves);

				GD.Print("Handling other player ", p.PlayerName);
			}
		}

		private void ApplyMoves(Player player, Dictionary<int, Vector2> moves)
		{
			foreach (var move in moves)
			{
				var forUnit = move.Key;
				var newTarget = move.Value;

				var unit = player.Units[forUnit];
				var unitPosition = new Point(unit.PositionX, unit.PositionY);
				var targetPosition = new Point((int)newTarget.x, (int)newTarget.y);

				BreadthFirstPathfinder.Search(this.Astar, unitPosition, unit.MoveDistance, out var result);

				if (result.ContainsKey(targetPosition))
				{
					unit.PositionX = targetPosition.X;
					unit.PositionY = targetPosition.Y;
				}
			}
		}

		public TurnData GetTurnData(int forPlayer)
		{
			return new TurnData
			{
				YourUnits = Players[forPlayer].Units,
				VisibleMap = GetVisibleMap(forPlayer),
				Players = Players.Where(a => a.Key != forPlayer).ToDictionary(a => a.Key, a => new Player
				{
					PlayerName = a.Value.PlayerName,
					Units = a.Value.Units.Where(b => IsVisible(Players[forPlayer], b.Value.PositionX, b.Value.PositionY)).ToDictionary(b => b.Key, b => b.Value)
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

		private static bool IsVisible(Player player, int x, int y)
		{
			foreach(var unit in player.Units.Values)
			{
				if ((Math.Abs(x - unit.PositionX) + Math.Abs(y - unit.PositionY)) <= 5){
					return true;
				}
			}

			return false;
		}
	}
}
