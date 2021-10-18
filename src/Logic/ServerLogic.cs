using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using IsometricGame.Logic.ScriptHelpers;
using IsometricGame.Repository;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace IsometricGame.Logic
{
    public class ServerLogic
    {
        private readonly GameLogic gameLogic;
        private readonly AccountRepository accountRepository;
        private readonly GamesRepository gamesRepository;

        public ServerLogic(
            GameLogic gameLogic, 
            AccountRepository accountRepository,
            GamesRepository gamesRepository)
        {
            this.gameLogic = gameLogic;
            this.accountRepository = accountRepository;
            this.gamesRepository = gamesRepository;
        }

        public void ProcessTick(float delta)
        {
            var games = this.gamesRepository.GetAllGames();
            foreach (var game in games)
            {
                this.gameLogic.CheckTimeout(game, delta);
            }
        }

        public void InitializeServer()
        {
            this.gamesRepository.ClearAllLobby();
            this.accountRepository.InitializeActiveLogins();
        }

        public bool Login(int clientId, string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            var credentials = this.accountRepository.LoadCredentials();
            var hash = Convert.ToBase64String(MD5.Create().ComputeHash(Encoding.Unicode.GetBytes($"{login}_{password}")));
            if (!credentials.ContainsKey(login))
            {
                credentials[login] = hash;
                this.accountRepository.AddActiveLogin(clientId, login);
                this.accountRepository.SaveCredentials(credentials);
                return true;
            }
            else if (credentials[login] == hash)
            {
                this.accountRepository.AddActiveLogin(clientId, login);
                return true;
            }
            else
            {
                return false;
            }
        }

        public string CreateLobby(int clientId)
        {
            var lobby = this.gamesRepository.CreateLobby();
            lobby.Creator = clientId;
            this.gamesRepository.AttachPlayerToGameLobby(clientId, lobby.Id);
            return lobby.Id;
        }

        public LobbyData.PlayerData JoinLobby(int clientId, string lobbyId)
        {
            var lobby = this.gamesRepository.FindByIdLobby(lobbyId);
            if (lobby == null)
            {
                return null;
            }

            this.gamesRepository.AttachPlayerToGameLobby(clientId, lobby.Id);
            var playerName = this.accountRepository.FindForClientActiveLogin(clientId);
            return this.AddPlayer(clientId, clientId, playerName);
        }

        public LobbyData.PlayerData AddBot(int clientId, Bot bot)
        {
            return this.AddPlayer(clientId, (int)bot, bot.ToString());
        }

        public bool IsCreator(int clientId)
        {
            var lobby = this.gamesRepository.FindForClientLobby(clientId);
            return lobby?.Creator == clientId;
        }

        private LobbyData.PlayerData AddPlayer(int clientId, int joinClientId, string playerName)
        {
            if (!this.IsCreator(clientId))
            {
                return null;
            }

            var lobby = this.gamesRepository.FindForClientLobby(clientId);
            var newPlayer = new LobbyData.PlayerData { ClientId = joinClientId, PlayerName = playerName };
            lobby.Players.Add(newPlayer);
            return newPlayer;
        }

        public void Logout(int clientId)
        {
            this.LeaveLobby(clientId);
            this.LeaveGame(clientId);
            this.accountRepository.RemoveActiveLogin(clientId);
        }

        private void LeaveGame(int clientId)
        {
            var game = this.gamesRepository.FindForClientGame(clientId);
            if (game == null)
            {
                return;
            }

            this.gameLogic.PlayerExitGame(game, clientId);
            this.gamesRepository.DetachPlayerFromGameLobby(clientId);
        }

        public void LeaveLobby(int clientId)
        {
            var lobby = this.gamesRepository.FindForClientLobby(clientId);
            if (lobby == null)
            {
                return;
            }

            lobby.Players.RemoveAll(a => a.ClientId == clientId);
            if (lobby.Players.All(a => a.ClientId < 0))
            {
                this.gamesRepository.RemoveLobby(lobby.Id);
            }

            this.gamesRepository.DetachPlayerFromGameLobby(clientId);
        }

        public bool UpdateConfig(int clientId, bool fullMapVisible, bool turnTimeoutEnaled, float turnTimeoutValue, int mapType)
        {
            if (!this.IsCreator(clientId))
            {
                return false;
            }

            var lobby = this.gamesRepository.FindForClientLobby(clientId);
            lobby.ServerConfiguration.FullMapVisible = fullMapVisible;
            lobby.ServerConfiguration.TurnTimeout = turnTimeoutEnaled ? (float?)turnTimeoutValue : null;
            lobby.ServerConfiguration.MapType = (MapGeneratingType)mapType;
            return true;
        }

        public GameData StartGame(int clientId)
        {
            if (!this.IsCreator(clientId))
            {
                return null;
            }

            var lobby = this.gamesRepository.FindForClientLobby(clientId);
            if (lobby == null)
            {
                return null;
            }

            var game = this.gameLogic.StartForLobby(lobby);

            this.gamesRepository.RemoveLobby(lobby.Id);
            this.gamesRepository.AddGame(game);
            return game;
        }

        public void PlayerMove(int clientId, TransferTurnDoneData turnData)
        {
            var game = this.gamesRepository.FindForClientGame(clientId);
            this.gameLogic.PlayerMove(game, clientId, turnData);

            foreach (var playerGameOver in game.PlayersGameOver)
            {
                this.gamesRepository.DetachPlayerFromGameLobby(playerGameOver.Key);
            }

            if (game.Players.Count == 0)
            {
                this.gamesRepository.RemoveGame(game.Id);
            }
        }

        public void ConnectToGame(int clientId, TransferConnectData connectData, Action<TransferInitialData> initialize, Action<TransferTurnData> turnDone)
        {
            var game = this.gamesRepository.FindForClientGame(clientId);
            this.gameLogic.Connect(game, clientId, connectData, initialize, turnDone);
        }
    }
}
