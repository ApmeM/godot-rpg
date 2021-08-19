using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.AppliedActions
{
    public class ChangeHpAppliedAction : IAppliedAction
    {
        private readonly ServerUnit unit;

        public int Value { get; private set; }

        public ChangeHpAppliedAction(int value, ServerUnit unit)
        {
            this.Value = value;
            this.unit = unit;
        }

        public void Apply()
        {
            this.unit.Hp += Value;
        }
    }
}
