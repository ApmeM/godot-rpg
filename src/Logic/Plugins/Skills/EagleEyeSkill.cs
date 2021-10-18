using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.Skills
{
    public class EagleEyeSkill : ISkill
    {
        public string Description => $"Eagle eye: \n Increase sight range: x2 times.";

        public Skill Skill => Skill.EagleEye;

        public void Apply(ServerPlayer player, ServerUnit unit)
        {
            unit.SightRange += 2;
        }
    }
}
