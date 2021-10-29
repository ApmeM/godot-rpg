using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using System.Collections.Generic;

namespace IsometricGame.Logic.ScriptHelpers
{
    public interface IAbility
    {
        Ability Ability { get; }
        AbilityType AbilityType { get; }
        string Description { get; }

        void HighliteMaze(Maze maze, Vector2 pos, ClientUnit currentUnit);

        List<IAppliedAction> Apply(ServerUnit value, GameData game, Vector2 abilityDirection);
    }
}
