using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.Skills
{
    public class VisionRangeSkill : ISkill
    {
        public void Apply(ServerPlayer player, ServerUnit unit)
        {
            unit.VisionRange += 2;
        }
    }
}
