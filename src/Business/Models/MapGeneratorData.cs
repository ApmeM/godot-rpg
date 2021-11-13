using Godot;
using IsometricGame.Logic.Enums;
using System.Collections.Generic;

namespace IsometricGame.Business.Models
{
    public struct MapGeneratorData
    {
        public List<Vector2> StartingPoints;
        public MapTile[,] Map;
    }
}
