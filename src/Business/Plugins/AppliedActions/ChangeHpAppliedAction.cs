using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.AppliedActions
{
    public class ChangeHpAppliedAction : IAppliedAction
    {
        private readonly ServerUnit unit;

        public long FullUnitId { get; private set; }
        public int Value { get; private set; }

        public ChangeHpAppliedAction(int value, ServerUnit unit)
        {
            this.Value = value;
            this.unit = unit;
            this.FullUnitId = UnitUtils.GetFullUnitId(unit);
        }

        public void Apply()
        {
            this.unit.Hp += Value;
        }
    }
}
