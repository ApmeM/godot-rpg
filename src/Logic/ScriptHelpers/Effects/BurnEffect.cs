using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.Effects
{
    public class BurnEffect : IEffect
    {
        public void ApplyFirstTurn(ServerUnit unit)
        {
        }

        public void ApplyEachTurn(ServerUnit unit)
        {
            if (unit.Hp > 0)
            {
                unit.Hp -= 1;
            }
        }

        public void ApplyLastTurn(ServerUnit unit)
        {
        }
    }
}
