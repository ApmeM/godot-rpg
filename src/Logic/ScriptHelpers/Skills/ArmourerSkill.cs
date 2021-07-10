using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.Skills
{
    public class ArmourerSkill : ISkill
    {
        public void Apply(ServerPlayer player, ServerUnit unit)
        {
            unit.MaxHp *= 2;
        }
    }
}
