using IsometricGame.Logic.Enums;
using System.Collections.Generic;

namespace IsometricGame.Logic.Models
{
    public class TransferInitialData
    {
        public int YourPlayerId;
        public List<TransferInitialPlayer> OtherPlayers;
        public List<TransferInitialUnit> YourUnits;
        public MapTile[,] VisibleMap;
    }
}
