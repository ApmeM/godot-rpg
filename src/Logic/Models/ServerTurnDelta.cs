using Godot;
using IsometricGame.Logic.ScriptHelpers;
using System.Collections.Generic;

namespace IsometricGame.Logic.Models
{
    public class ServerTurnDelta
    {
        public Vector2? AbilityDirection;
        public Vector2? AbilityFrom;
        public List<IAbilityAction> Actions = new List<IAbilityAction>();
    }
}