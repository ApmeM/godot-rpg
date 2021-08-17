using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.Skills
{
    public class BallisticsSkill : ISkill
    {
        public string Description => $"Ballistics: \n Increase AOE radius for \n  all AOE abilities: x1.5 times.";
        public void Apply(ServerPlayer player, ServerUnit unit)
        {
            unit.AOEAttackRadius *= 1.5f;
        }
    }
}
