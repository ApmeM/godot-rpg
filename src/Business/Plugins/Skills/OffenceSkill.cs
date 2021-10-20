using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.Skills
{
    public class OffenceSkill : ISkill
    {
        public string Description => $"Offence: \n Increase attack power: x1.5 times.";

        public Skill Skill => Skill.Offence;

        public void Apply(ServerPlayer player, ServerUnit unit)
        {
            unit.AttackPower *= 1.5f;
        }
    }
}
