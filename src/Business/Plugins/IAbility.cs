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

        bool IsApplicable(MapGraphData astar, ServerUnit actionUnit, ServerUnit targetUnit, Vector2 abilityDirection);
        List<IAppliedAction> Apply(ServerUnit actionUnit, ServerUnit targetUnit);
        List<IAppliedAction> ApplyCost(ServerUnit actionUnit);
    }
}
