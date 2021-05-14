using BrainAI.Pathfinding;
using BrainAI.Pathfinding.AStar;
using Godot;
using MazeGenerators;
using System;
using System.Collections.Generic;

namespace IsometricGame.Logic
{
    public class Server
    {
        private RoomMazeGenerator.Result Map;
        private AstarGridGraph Astar;
        private List<Player> Players = new List<Player>();

        public void Start()
        {
            var generator = new RoomMazeGenerator();
            this.Map = generator.Generate(new RoomMazeGenerator.Settings
            {
                Width = 51,
                Height = 51
            });

            this.Astar = new AstarGridGraph(21, 21);
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
            Players.Add(new Player
            {
                PlayerName = playerName,
                PositionX = Map.Rooms[idx].X + Map.Rooms[idx].Width / 2,
                PositionY = Map.Rooms[idx].Y + Map.Rooms[idx].Height / 2,
            });

            return idx;
        }

        public class InitialData
        {
            public int PositionX;
            public int PositionY;
            public int MapWidth;
            public int MapHeight;
        }

        public InitialData GetInitialData(int forPlayer)
        {
            return new InitialData {
                MapWidth = Map.Regions.GetLength(0),
                MapHeight = Map.Regions.GetLength(1),
                PositionX = Players[forPlayer].PositionX,
                PositionY = Players[forPlayer].PositionY
            };
        }

        public void PlayerMove(int forPlayer, Vector2 newTarget)
        {
            Players[forPlayer].PositionX = (int)newTarget.x;
            Players[forPlayer].PositionY = (int)newTarget.y;
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
                    if ((Math.Abs(x - player.PositionX) + Math.Abs(y - player.PositionY)) > 5)
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
    }
}
