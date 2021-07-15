using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.Abilities.Action
{
    public class ChangeHpAbilityAction : IAbilityAction
    {
        private readonly int value;

        public ChangeHpAbilityAction(int value)
        {
            this.value = value;
        }

        public void Apply(ServerUnit unit)
        {
            unit.Hp += value;
        }
    }
}
