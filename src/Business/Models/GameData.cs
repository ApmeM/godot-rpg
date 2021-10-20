using IsometricGame.Logic.ScriptHelpers;
using MazeGenerators.Utils;
using System;
using System.Collections.Generic;

namespace IsometricGame.Logic.Models
{
    public class GameData
    {
        public string Id { get; private set; }

        public GeneratorResult Map;
		public MapGraphData Astar;
		public readonly Dictionary<int, ServerPlayer> Players = new Dictionary<int, ServerPlayer>();
		public readonly Dictionary<int, ServerPlayer> PlayersGameOver = new Dictionary<int, ServerPlayer>();
		public readonly Dictionary<int, TransferTurnDoneData> PlayersMove = new Dictionary<int, TransferTurnDoneData>();
		
		public ServerConfiguration Configuration;
		public float? Timeout;

        public GameData(string id)
        {
            this.Id = id;
        }
    }
}