using IsometricGame.Logic.Enums;
using IsometricGame.Logic.ScriptHelpers;
using System.Collections.Generic;

namespace IsometricGame.Logic.Models
{
    public class ClientUnit
    {
        public int UnitId;
        public int PlayerId;
        public int? MoveDistance;
        public int? SightRange;
        public int? AttackDistance;
        public int? AttackRadius;
        public UnitType UnitType;
        public int MaxHp;
        public int Hp;
        public Dictionary<Usable, IUsable> Usables;
    }
}