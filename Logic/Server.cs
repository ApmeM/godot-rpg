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
                Width = 51,
                Height = 51
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
        }

        public int Connect(string playerName)
        {
            var idx = Players.Count;
            var player = new Player
            {
                PlayerName = playerName,
            };

            var centerX = Map.Rooms[idx].X + Map.Rooms[idx].Width / 2;
            var centerY = this.Map.Rooms[idx].Y + this.Map.Rooms[idx].Height / 2;

            player.Units.Add(1, new Unit { UnitId = 1, PositionX = centerX - 1, PositionY = centerY });
            player.Units.Add(2, new Unit { UnitId = 2, PositionX = centerX + 1, PositionY = centerY });
            player.Units.Add(3, new Unit { UnitId = 3, PositionX = centerX, PositionY = centerY + 1 });
            player.Units.Add(4, new Unit { UnitId = 4, PositionX = centerX, PositionY = centerY - 1 });

            this.Players.Add(idx, player);

            return idx;
        }

        public InitialData GetInitialData(int forPlayer)
        {
            return new InitialData {
                MapWidth = Map.Regions.GetLength(0),
                MapHeight = Map.Regions.GetLength(1),
                Units = Players[forPlayer].Units.Values.ToList()
            };
        }

        public void PlayerMove(int forPlayer, int forUnit, Vector2 newTarget)
        {
            var unit = Players[forPlayer].Units[forUnit];
            var unitPosition = new Point(unit.PositionX, unit.PositionY);
            var targetPosition = new Point((int)newTarget.x, (int)newTarget.y);

            BreadthFirstPathfinder.Search(this.Astar, unitPosition, unit.MoveDistance, out var result);

            if (result.ContainsKey(targetPosition))
            {
                unit.PositionX = targetPosition.X;
                unit.PositionY = targetPosition.Y;
            }
        }

        public Player GetPlayer(int forPlayer)
        {
            return Players[forPlayer];
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
