using IsometricGame.Logic.Enums;

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
    }
}