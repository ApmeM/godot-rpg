using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers
{
    public interface IUnitType
    {
        UnitType UnitType { get; }
        void Apply(ServerUnit unit);
        void Initialize(ServerUnit unit);
        SpriteFrames GetFrames();
    }
}
