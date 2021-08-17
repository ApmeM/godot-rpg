using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.Skills
{
    public class LogisticsSkill : ISkill
    {
        public string Description => $"Logistics: \n Increase move distance: 1";

        public void Apply(ServerPlayer player, ServerUnit unit)
        {
            unit.MoveDistance += 1;
        }
    }
}
