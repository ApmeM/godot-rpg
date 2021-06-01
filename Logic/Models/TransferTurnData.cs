using IsometricGame.Logic.Enums;
using System.Collections.Generic;

namespace IsometricGame.Logic.Models
{
    public class TransferTurnData
    {
        public Dictionary<int, TransferTurnUnit> YourUnits;
        public MapTile[,] VisibleMap;
        public Dictionary<int, TransferTurnPlayer> OtherPlayers;
    }
}
