using System.Collections.Generic;

namespace IsometricGame.Logic.Models
{
    public class TurnData
    {
        public Dictionary<int, Unit> YourUnits;
        public MapTile[,] VisibleMap;
        public Dictionary<int, Player> Players;
    }
}
