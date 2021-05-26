﻿using System.Collections.Generic;

namespace IsometricGame.Logic.Models
{
    public class InitialData
    {
        public List<Unit> YourUnits;
        public int MapWidth;
        public int MapHeight;
        public MapTile[,] VisibleMap;
        public Dictionary<int, Player> Players;
    }
}
