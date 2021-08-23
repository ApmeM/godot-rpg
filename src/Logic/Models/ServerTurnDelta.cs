using Godot;
using System.Collections.Generic;

namespace IsometricGame.Logic.Models
{
    public class ServerTurnDelta
    {
        public Vector2? AbilityDirection;
        public Vector2? AbilityFrom;
        public List<int> HpChanges = new List<int>();
        public List<int> MpChanges = new List<int>();
    }
}