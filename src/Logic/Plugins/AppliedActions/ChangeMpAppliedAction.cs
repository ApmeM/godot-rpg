using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.AppliedActions
{
    public class ChangeMpAppliedAction : IAppliedAction
    {
        private readonly ServerUnit unit;

        public long FullUnitId { get; private set; }
        public int Value { get; private set; }

        public ChangeMpAppliedAction(int value, ServerUnit unit)
        {
            this.Value = value;
            this.unit = unit;
            this.FullUnitId = UnitUtils.GetFullUnitId(unit);
        }

        public void Apply()
        {
            this.unit.Mp += Value;
        }
    }
}
