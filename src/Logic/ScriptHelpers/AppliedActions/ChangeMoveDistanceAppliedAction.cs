using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers.AppliedActions
{
    public class ChangeMoveDistanceAppliedAction : IAppliedAction
    {
        private readonly int value;
        private readonly ServerUnit unit;

        public ChangeMoveDistanceAppliedAction(int value, ServerUnit unit)
        {
            this.value = value;
            this.unit = unit;
        }

        public void Apply()
        {
            this.unit.MoveDistance += value;
        }
    }
}
