using System.Collections.Generic;

namespace IsometricGame.Logic.Models
{
    public class LobbyData
    {
        public string Id { get; private set; }
        public int Creator;
        public List<PlayerData> Players = new List<PlayerData>();
        public ServerConfiguration ServerConfiguration = new ServerConfiguration();

        public LobbyData(string id)
        {
            this.Id = id;
        }

        public class PlayerData
        {
            public int ClientId;
            public string PlayerName;
        }
    }
}