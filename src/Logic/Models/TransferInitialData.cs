using Godot;
using IsometricGame.Logic.Enums;
using System.Collections.Generic;

namespace IsometricGame.Logic.Models
{
    public class TransferInitialData
    {
        public List<OtherPlayerData> OtherPlayers;
        public List<YourUnitsData> YourUnits;
        public MapTile[,] VisibleMap;

        public class OtherPlayerData
        {
            public int PlayerId;
            public string PlayerName;
            public List<OtherUnitsData> Units;
        }

        public class OtherUnitsData
        {
            public int Id;
            public UnitType UnitType;
            public int MaxHp;
        }

        public class YourUnitsData
        {
            public Vector2 Position;
            public int UnitId;
            public int MoveDistance;
            public int SightRange;
            public int AttackDistance;
            public int AttackRadius;
            public int AttackPower;
            public UnitType UnitType;
            public int MaxHp;
            public List<Ability> Abilities;
        }
    }
}
