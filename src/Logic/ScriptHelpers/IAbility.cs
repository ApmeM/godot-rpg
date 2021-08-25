﻿using Godot;
using IsometricGame.Logic.Models;
using System.Collections.Generic;

namespace IsometricGame.Logic.ScriptHelpers
{
    public interface IAbility
    {
        string Description { get; }
        bool TargetUnit { get; }
        void HighliteMaze(Maze maze, Vector2 pos, ClientUnit currentUnit);

        bool IsApplicable(VectorGridGraph astar, ServerUnit actionUnit, ServerUnit targetUnit, Vector2 abilityDirection);
        List<IAppliedAction> Apply(ServerUnit actionUnit, ServerUnit targetUnit);
        List<IAppliedAction> ApplyCost(ServerUnit actionUnit);
    }
}
