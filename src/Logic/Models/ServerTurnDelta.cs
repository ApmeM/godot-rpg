using Godot;
using IsometricGame.Logic.Enums;

namespace IsometricGame.Logic.Models
{
    public class ServerTurnDelta
    {
        public Vector2 MovedFrom;
        public Vector2 MovedTo;
        public Vector2? AbilityDirection;
        public Vector2? AbilityFrom;
        public Ability Ability;
    }
}