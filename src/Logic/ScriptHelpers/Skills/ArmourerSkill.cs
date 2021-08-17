using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.Skills
{
    public class ArmourerSkill : ISkill
    {
        public string Description => $"Armourer: \n Increase max HP: x2 times.";
        public void Apply(ServerPlayer player, ServerUnit unit)
        {
            unit.MaxHp *= 2;
        }
    }
}
