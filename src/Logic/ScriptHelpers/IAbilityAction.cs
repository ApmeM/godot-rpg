using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers
{
    public interface IAbilityAction
    {
        void Apply(ServerUnit unit);
    }
}
