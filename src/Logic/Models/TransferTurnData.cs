using Godot;
using IsometricGame.Logic.Enums;
using System.Collections.Generic;

namespace IsometricGame.Logic.Models
{
    public class TransferTurnData
    {
        public Dictionary<int, YourUnitsData> YourUnits;
        public MapTile[,] VisibleMap;
        public Dictionary<int, OtherPlayersData> OtherPlayers;

        public class YourUnitsData
        {
            public Vector2 Position;
            public Vector2? AttackDirection;
            public int Hp;
            public Vector2? AttackFrom;
        }

        public class OtherPlayersData
        {
            public Dictionary<int, OtherUnitsData> Units;
        }
        public class OtherUnitsData
        {
            public Vector2 Position;
            public Vector2? AttackDirection;
            public int Hp;
            public Vector2? AttackFrom;
        }
    }
}
