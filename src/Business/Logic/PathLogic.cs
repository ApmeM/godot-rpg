using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.ScriptHelpers;
using System;
using System.Collections.Generic;

namespace IsometricGame.Business.Logic
{
    public class PathLogic
    {
        public MapGraphData GetGraphData(MapTile[,] map, bool isFly)
        {
            var graphData = new MapGraphData(map.GetLength(0), map.GetLength(1));
            this.RefreshGraphData(map, isFly, graphData);
            return graphData;
        }

        public void RefreshGraphData(MapTile[,] map, bool isFly, MapGraphData graphData)
        {
            graphData.Paths.Clear();
            for (var x = 0; x < map.GetLength(0); x++)
                for (var y = 0; y < map.GetLength(1); y++)
                {
                    switch (map[x, y])
                    {
                        case MapTile.Door:
                            {
                                graphData.Paths.Add(new Godot.Vector2(x, y));
                                graphData.Paths.Add(new Godot.Vector2(x, y));
                                break;
                            }
                        case MapTile.Path:
                        case MapTile.StartPoint:
                            {
                                graphData.Paths.Add(new Godot.Vector2(x, y));
                                graphData.Paths.Add(new Godot.Vector2(x, y));
                                break;
                            }
                        case MapTile.Pit:
                            {
                                if (isFly)
                                {
                                    graphData.Paths.Add(new Godot.Vector2(x, y));
                                }
                                break;
                            }
                        case MapTile.Unknown:
                        case MapTile.Wall:
                            {
                                break;
                            }
                        default:
                            {
                                throw new Exception($"Unhandled MapTile {map[x, y]}");
                            }
                    }
                }
        }

        public List<Vector2> CollectStartingPoint(MapTile[,] map)
        {
            var startingPoints = new List<Vector2>();
            for (var x = 0; x < map.GetLength(0); x++)
                for (var y = 0; y < map.GetLength(1); y++)
                {
                    switch (map[x, y])
                    {
                        case MapTile.StartPoint:
                            {
                                startingPoints.Add(new Godot.Vector2(x, y));
                                break;
                            }
                        default:
                            {
                                break;
                            }
                    }
                }

            return startingPoints;
        }
    }
}
