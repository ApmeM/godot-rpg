using IsometricGame.Logic.Models;
using System.Collections.Generic;

namespace IsometricGame.Logic.ScriptHelpers
{
    public interface IEffect
    {
        string Description { get; }
        List<IAppliedAction> Apply(ServerUnit unit);
    }
}
