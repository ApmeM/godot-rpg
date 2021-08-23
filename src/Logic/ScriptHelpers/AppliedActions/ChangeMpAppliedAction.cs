using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.AppliedActions
{
    public class ChangeMpAppliedAction : IAppliedAction
    {
        private readonly ServerUnit unit;

        public int Value { get; private set; }

        public ChangeMpAppliedAction(int value, ServerUnit unit)
        {
            this.Value = value;
            this.unit = unit;
        }

        public void Apply()
        {
            this.unit.Mp += Value;
        }
    }
}
