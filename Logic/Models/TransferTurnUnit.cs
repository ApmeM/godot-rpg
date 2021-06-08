using Godot;

namespace IsometricGame.Logic.Models
{
    public class TransferTurnUnit
    {
        public Vector2 Position;
        public Vector2? AttackDirection;
        public int? HpReduced;
        public Vector2? AttackFrom;
        public bool IsDead;
    }
}