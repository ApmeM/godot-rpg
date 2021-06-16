using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.Skills
{
    public class MoveRangeSkill : ISkill
    {
        public void Apply(ServerPlayer player, ServerUnit unit)
        {
            unit.MoveDistance += 1;
        }
    }
}
