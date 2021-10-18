using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;

namespace IsometricGame.Logic.ScriptHelpers
{
    public interface IBot
    {
        Bot Bot { get; }
        void StartGame(GameData game, int myId);
    }
}
