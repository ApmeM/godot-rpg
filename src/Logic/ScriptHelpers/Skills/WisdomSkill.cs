using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.Skills
{
    public class WisdomSkill : ISkill
    {
        public string Description => $"Wisdom: \n Increase magic power: x1.5 times.";

        public void Apply(ServerPlayer player, ServerUnit unit)
        {
            unit.MagicPower *= 1.5f;
        }
    }
}
