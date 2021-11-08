using Godot;
using IsometricGame.Logic.Enums;
using System.Collections.Generic;

namespace IsometricGame.Logic.Models
{
    public class ServerTurnDelta
    {
        public List<(Ability, Vector2)> AppliedAbilities = new List<(Ability, Vector2)>();
        public List<int> HpChanges = new List<int>();
        public List<int> MpChanges = new List<int>();
        public List<(Ability, Vector2)> ExecutedAbilities = new List<(Ability, Vector2)>();
    }
}