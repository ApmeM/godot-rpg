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
            public int? HpReduced;
            public Vector2? AttackFrom;
            public bool IsDead;
        }

        public class OtherPlayersData
        {
            public Dictionary<int, OtherUnitsData> Units;
        }
        public class OtherUnitsData
        {
            public Vector2 Position;
            public Vector2? AttackDirection;
            public int? HpReduced;
            public Vector2? AttackFrom;
            public bool IsDead;
        }
    }
}
