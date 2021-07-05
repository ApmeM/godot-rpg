using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.Skills
{
    public class OffenceSkill : ISkill
    {
        public void Apply(ServerPlayer player, ServerUnit unit)
        {
            unit.AttackDamage += 5;
        }
    }
}
