using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.Effects
{
    public class HasteEffect : IEffect
    {
        public void ApplyFirstTurn(ServerUnit unit)
        {
            unit.MoveDistance = (int)(unit.MoveDistance * 1.5f);
        }

        public void ApplyEachTurn(ServerUnit unit)
        {
        }

        public void ApplyLastTurn(ServerUnit unit)
        {
            unit.MoveDistance = (int)(unit.MoveDistance / 1.5f);
        }
    }
}
