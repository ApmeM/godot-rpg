using System.Collections.Generic;

namespace IsometricGame.Logic.Models
{
    public class LobbyData
    {
        public List<int> Players = new List<int>();
        public int Creator;
        
        public Server Server;
    }
}