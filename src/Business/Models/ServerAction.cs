using Godot;
using IsometricGame.Logic.Enums;

namespace IsometricGame.Business.Models
{
    public struct ServerAction
    {
        public Ability Ability;
        public Vector2? AbilityDirection;
        public long? AbilityFullUnitId;
        public int PlayerId;
        public int UnitId;
        public int ExecuteOrder;

        public ServerAction(ServerAction origin)
        {
            this.Ability = origin.Ability;
            this.AbilityDirection = origin.AbilityDirection;
            this.AbilityFullUnitId = origin.AbilityFullUnitId;
            this.PlayerId = origin.PlayerId;
            this.UnitId = origin.UnitId;
            this.ExecuteOrder = origin.ExecuteOrder;
        }
    }
}
