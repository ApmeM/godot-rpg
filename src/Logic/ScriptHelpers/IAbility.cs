using Godot;
using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers
{
    public interface IAbility
    {
        bool TargetUnit { get; }
        void HighliteMaze(Maze maze, Vector2 pos, ClientUnit currentUnit);
        bool IsApplicable(VectorGridGraph astar, ServerPlayer actionPlayer, ServerUnit actionUnit, ServerPlayer targetPlayer, ServerUnit targetUnit, Vector2 abilityDirection);
        void Apply(ServerUnit actionUnit, ServerUnit targetUnit);
    }
}
