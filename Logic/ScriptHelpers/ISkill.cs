using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers
{
    public interface ISkill
    {
        void Apply(ServerPlayer player, ServerUnit unit);
    }
}
