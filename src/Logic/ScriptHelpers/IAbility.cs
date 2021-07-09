using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers
{
    public interface IAbility
    {
        void HighliteMaze(Maze maze, Vector2 pos, ClientUnit currentUnit);
        bool IsApplicable(ServerPlayer actionPlayer, ServerUnit actionUnit, ServerPlayer targetPlayer, ServerUnit targetUnit);
        void Apply(ServerUnit actionUnit, ServerUnit targetUnit);
        bool IsInRange(ServerUnit actionUnit, ServerUnit targetUnit, Vector2 abilityDirection);
    }
}
