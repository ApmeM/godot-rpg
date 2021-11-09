using IsometricGame.Logic.ScriptHelpers;
using Godot;
using System.Collections.Generic;
using IsometricGame.Logic.Enums;

namespace IsometricGame.Logic.Models
{
    public class GameData
    {
        public string Id { get; private set; }

        public ServerConfiguration Configuration;

        public List<Vector2> StartingPoints;
        public MapTile[,] Map;
        public MapGraphData AstarMove;
		public MapGraphData AstarFly;

        public readonly Dictionary<int, ServerPlayer> Players = new Dictionary<int, ServerPlayer>();
		public readonly Dictionary<int, ServerPlayer> PlayersGameOver = new Dictionary<int, ServerPlayer>();
		public readonly Dictionary<int, TransferTurnDoneData> PlayersMove = new Dictionary<int, TransferTurnDoneData>();
		
		public float? Timeout;
        
        public GameData(string id)
        {
            this.Id = id;
        }
    }
}