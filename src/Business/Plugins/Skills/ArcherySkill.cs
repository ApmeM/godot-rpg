using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.Skills
{
    public class ArcherySkill : ISkill
    {
        public string Description => $"Archery: \n Increase ranged attack \n  distance for all abilities: x1.5 times";

        public Skill Skill => Skill.Archery;

        public void Apply(ServerPlayer player, ServerUnit unit)
        {
            unit.RangedAttackDistance *= 1.5f;
        }
    }
}
