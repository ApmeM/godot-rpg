using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using System.Collections.Generic;

namespace IsometricGame.Logic.ScriptHelpers
{
    public interface IEffect
    {
        Effect Effect { get; }
        string Description { get; }
        List<IAppliedAction> Apply(ServerUnit unit);
    }
}
