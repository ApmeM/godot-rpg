using Godot;
using IsometricGame.Logic.Enums;

namespace IsometricGame.Logic.Models
{
    public class ServerTurnDelta
    {
        public Vector2 MovedFrom;
        public Vector2 MovedTo;
        public Vector2? UsableDirection;
        public Vector2? UsableFrom;
        public Usable Usable;
    }
}