using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.Effects
{
    public class BurnEffect : IEffect
    {
        public void Apply(ServerUnit unit)
        {
            if (unit.Hp > 0)
            {
                unit.Hp -= 1;
            }
        }
    }
}
