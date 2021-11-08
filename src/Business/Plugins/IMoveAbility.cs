using Godot;
using System.Collections.Generic;

namespace IsometricGame.Logic.ScriptHelpers
{
    public interface IMoveAbility : IAbility
    {
        bool IsBasicMove { get; }
        void MoveBy(Maze maze, Vector2 currentPosition, Vector2 newTarget, Queue<Vector2> currentPath);
    }
}
