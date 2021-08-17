using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers
{
    public interface ISkill
    {
        string Description { get; }
        void Apply(ServerPlayer player, ServerUnit unit);
    }
}
