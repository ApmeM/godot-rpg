using IsometricGame.Logic.ScriptHelpers;
using MazeGenerators;
using System;
using System.Collections.Generic;

namespace IsometricGame.Logic.Models
{
    public class GameData
    {
		public RoomMazeGenerator.Result Map;
		public VectorGridGraph Astar;
		public readonly Dictionary<int, ServerPlayer> Players = new Dictionary<int, ServerPlayer>();
		public readonly Dictionary<int, ServerPlayer> PlayersGameOver = new Dictionary<int, ServerPlayer>();
		public readonly Dictionary<int, TransferTurnDoneData> PlayersMove = new Dictionary<int, TransferTurnDoneData>();
		
		public ServerConfiguration Configuration;
		public float? Timeout;
		public readonly Dictionary<int, Action<TransferInitialData>> InitializeMethods = new Dictionary<int, Action<TransferInitialData>>();
        public readonly Dictionary<int, Action<TransferTurnData>> TurnDoneMethods = new Dictionary<int, Action<TransferTurnData>>();
	}
}