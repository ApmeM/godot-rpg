using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers
{
    public interface IEffect
    {
        void ApplyFirstTurn(ServerUnit value);
        void ApplyEachTurn(ServerUnit unit);
        void ApplyLastTurn(ServerUnit value);
    }
}
