using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using System.Collections.Generic;

namespace IsometricGame.Logic.ScriptHelpers
{
    public interface IAbility
    {
        Ability Ability { get; }
        string Description { get; }
        bool TargetUnit { get; }
        
        void HighliteMaze(Maze maze, Vector2 pos, ClientUnit currentUnit);

        List<IAppliedAction> Apply(ServerUnit value, GameData game, Vector2 abilityDirection);
    }
}
