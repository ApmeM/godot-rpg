using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers
{
    public interface IEffect
    {
        string Description { get; }
        void Apply(ServerUnit unit);
    }
}
