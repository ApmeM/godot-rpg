using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers
{
    public interface ISkill
    {
        Skill Skill { get; }
        string Description { get; }
        void Apply(ServerPlayer player, ServerUnit unit);
    }
}
