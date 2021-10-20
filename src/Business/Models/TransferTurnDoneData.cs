using Godot;
using IsometricGame.Logic.Enums;
using System.Collections.Generic;

namespace IsometricGame.Logic
{
    public class TransferTurnDoneData
    {
        public Dictionary<int, UnitActionData> UnitActions = new Dictionary<int, UnitActionData>();
        
        public class UnitActionData
        {
            public Vector2? Move;
            public Vector2? AbilityDirection;
            public Ability Ability;
            public long AbilityFullUnitId;
        }
    }
}