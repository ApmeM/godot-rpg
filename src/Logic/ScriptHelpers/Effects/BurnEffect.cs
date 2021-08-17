using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.Effects
{
    public class BurnEffect : IEffect
    {
        public string Description => "Burn effect: \n  Damage: 1";

        public void Apply(ServerUnit unit)
        {
            if (unit.Hp > 0)
            {
                unit.Hp -= 1;
            }
        }
    }
}
