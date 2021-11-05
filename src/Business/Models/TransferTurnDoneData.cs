using Godot;
using IsometricGame.Logic.Enums;
using System.Collections.Generic;

namespace IsometricGame.Logic
{
    public class TransferTurnDoneData
    {
        public Dictionary<int, List<UnitActionData>> UnitActions = new Dictionary<int, List<UnitActionData>>();
        
        public class UnitActionData
        {
            public Vector2? AbilityDirection;
            public Ability Ability;
            public long? AbilityFullUnitId;
        }
    }
}