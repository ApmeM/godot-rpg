namespace IsometricGame.Logic.ScriptHelpers
{
    public interface IAppliedAction
    {
        void Apply(System.Collections.Generic.Dictionary<long, Models.ServerTurnDelta> unitsTurnDelta);
    }
}
