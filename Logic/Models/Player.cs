using System.Collections.Generic;

namespace IsometricGame.Logic.Models
{
    public class Player
    {
        public string PlayerName;
        public Dictionary<int, Unit> Units = new Dictionary<int, Unit>();
    }
}