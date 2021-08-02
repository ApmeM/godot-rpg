using System.Collections.Generic;

namespace IsometricGame.Logic.Models
{
    public class LobbyData
    {
        public List<PlayerData> Players = new List<PlayerData>();
        public int Creator;
        
        public ServerLogic Server;

        public class PlayerData
        {
            public int ClientId;
            public string PlayerName;
        }
    }
}