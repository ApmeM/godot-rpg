using System.Collections.Generic;

namespace IsometricGame.Logic.Models
{
    public class ServerPlayer
    {
        public int PlayerId;
        public string PlayerName;
        public Dictionary<int, ServerUnit> Units = new Dictionary<int, ServerUnit>();
        public bool IsGameOver;
    }
}