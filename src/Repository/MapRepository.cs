using IsometricGame.Business.Models;
using IsometricGame.Logic.Enums;
using MazeGenerators;
using MazeGenerators.Utils;
using System;
using System.Linq;

namespace IsometricGame.Logic.ScriptHelpers
{
    public class MapRepository
    {
        public const int JunctionTileId = 4;
        public const int RoomTileId     = 3;
        public const int MazeTileId     = 2;
        public const int WallTileId     = 1;
        public const int EmptyTileId    = 0;
        
        public MapGeneratorData CreateForType(MapGeneratingType mapType)
        {
            var map = this.CreateForTypeInternal(mapType);
            var game = new MapGeneratorData
            {
                StartingPoints = map.Rooms.Select(a => new Godot.Vector2(a.X + a.Width / 2, a.Y + a.Height / 2)).ToList(),
                Map = new MapTile[map.Paths.GetLength(0), map.Paths.GetLength(1)],
                AstarMove = new MapGraphData(map.Paths.GetLength(0), map.Paths.GetLength(1)),
                AstarFly = new MapGraphData(map.Paths.GetLength(0), map.Paths.GetLength(1))
            };

            for (var x = 0; x < map.Paths.GetLength(0); x++)
                for (var y = 0; y < map.Paths.GetLength(1); y++)
                {
                    switch (map.Paths[x, y])
                    {
                        case JunctionTileId:
                            {
                                game.Map[x, y] = MapTile.Door;
                                game.AstarMove.Paths.Add(new Godot.Vector2(x, y));
                                game.AstarFly.Paths.Add(new Godot.Vector2(x, y));
                                break;
                            }
                        case MazeTileId:
                        case RoomTileId:
                            {
                                game.Map[x, y] = MapTile.Path;
                                game.AstarMove.Paths.Add(new Godot.Vector2(x, y));
                                game.AstarFly.Paths.Add(new Godot.Vector2(x, y));
                                break;
                            }
                        case WallTileId:
                            {
                                game.Map[x, y] = MapTile.Wall;
                                break;
                            }
                        case EmptyTileId:
                            {
                                game.Map[x, y] = MapTile.Pit;
                                game.AstarFly.Paths.Add(new Godot.Vector2(x, y));
                                break;
                            }
                    }
                }
            return game;
        }

        private GeneratorResult CreateForTypeInternal(MapGeneratingType mapType)
        {
            var result = new GeneratorResult();

            switch (mapType)
            {
                case MapGeneratingType.Arena:
                    {
                        var settings = new GeneratorSettings
                        {
                            Width = 17,
                            Height = 17,
                            Mirror = GeneratorSettings.MirrorDirection.Rotate,
                            EmptyTileId = EmptyTileId,
                            WallTileId = WallTileId,
                            MazeTileId = MazeTileId,
                            RoomTileId = RoomTileId,
                            JunctionTileId = JunctionTileId,
                        };

                        CommonAlgorithm.GenerateField(result, settings);
                        RoomGeneratorAlgorithm.AddRoom(result, settings, new Rectangle(1, 1, settings.Width - 2, settings.Height - 2));

                        MirroringAlgorithm.Mirror(result, settings);
                        WallSurroundingAlgorithm.BuildWalls(result, settings);

                        for (var x = 12; x < 18; x++)
                            for (var y = 12; y < 18; y++)
                            {
                                result.Paths[x, y] = EmptyTileId;
                            }

                        break;
                    }
                case MapGeneratingType.Random:
                    {
                        var settings = new GeneratorSettings
                        {
                            Width = 35,
                            Height = 35,
                            MinRoomSize = 5,
                            MaxRoomSize = 9,
                            EmptyTileId = EmptyTileId,
                            WallTileId = WallTileId,
                            MazeTileId = MazeTileId,
                            RoomTileId = RoomTileId,
                            JunctionTileId = JunctionTileId,
                        };

                        CommonAlgorithm.GenerateField(result, settings);
                        RoomGeneratorAlgorithm.GenerateRooms(result, settings);
                        TreeMazeBuilderAlgorithm.GrowMaze(result, settings);
                        RegionConnectorAlgorithm.GenerateConnectors(result, settings);
                        DeadEndRemoverAlgorithm.RemoveDeadEnds(result, settings);
                        WallSurroundingAlgorithm.BuildWalls(result, settings);
                        break;
                    }
                case MapGeneratingType.Random2:
                    {
                        var settings = new GeneratorSettings
                        {
                            Width = 35,
                            Height = 35,
                            MinRoomSize = 5,
                            MaxRoomSize = 9,
                            PreventOverlappedRooms = true,
                            TargetRoomCount = 4,
                            EmptyTileId = EmptyTileId,
                            WallTileId = WallTileId,
                            MazeTileId = MazeTileId,
                            RoomTileId = RoomTileId,
                            JunctionTileId = JunctionTileId,
                        };

                        CommonAlgorithm.GenerateField(result, settings);
                        RoomGeneratorAlgorithm.GenerateRooms(result, settings);
                        for (var i = 1; i < result.Rooms.Count; i++)
                        {
                            var from = result.Rooms[i - 1];
                            var to = result.Rooms[i];

                            var minX = Math.Min(from.X + from.Width / 2, to.X + to.Width / 2);
                            var maxX = Math.Max(from.X + from.Width / 2, to.X + to.Width / 2);
                            var minY = Math.Min(from.Y + from.Height / 2, to.Y + to.Height / 2);
                            var maxY = Math.Max(from.Y + from.Height / 2, to.Y + to.Height / 2);

                            var maxDiff = Math.Max(maxY - minY, maxX - minX);

                            if (maxDiff == 0)
                            {
                                continue;
                            }

                            var sign = -1;

                            if (minX == from.X + from.Width / 2 && minY == from.Y + from.Height / 2 ||
                                minX == to.X + to.Width / 2 && minY == to.Y + to.Height / 2)
                            {
                                sign = 1;
                            }

                            for (var x = 0; x <= maxDiff; x++)
                            {
                                var posX = (maxX - minX) * x / maxDiff + minX;
                                var posY = sign * (maxY - minY) * x / maxDiff + (sign > 0 ? minY : maxY);
                                result.Paths[posX, posY] = settings.MazeTileId;
                                result.Paths[posX-1, posY] = settings.MazeTileId;
                                result.Paths[posX+1, posY] = settings.MazeTileId;
                                result.Paths[posX, posY-1] = settings.MazeTileId;
                                result.Paths[posX, posY+1] = settings.MazeTileId;
                            }
                        }
                        WallSurroundingAlgorithm.BuildWalls(result, settings);
                        break;
                    }

            }

            return result;
        }
    }
}
