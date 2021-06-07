using Godot;

namespace IsometricGame.Logic.Models
{
    public class ServerTurnDelta
    {
        public Vector2 MovedFrom;
        public Vector2 MovedTo;
        public Vector2? AttackDirection;
        public int HpBefore;
        public int? HpChanges;
        public Vector2? AttackFrom;
    }
}