using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers
{
    public interface IUnitType
    {
        void Apply(ServerUnit unit);
        void Initialize(ServerUnit unit);
    }
}
