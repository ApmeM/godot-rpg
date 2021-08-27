using Godot;
using IsometricGame.Logic;
using IsometricGame.Logic.Models;
using IsometricGame.Logic.ScriptHelpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

public class Communicator : Node
{
    private const int BotId = -1;
    private const string BotName = "Bot";
    private const int ConnectionPort = 12345;

    private static int LobbyIndex = 0;

    public readonly Dictionary<string, LobbyData> Lobbies = new Dictionary<string, LobbyData>();
    public readonly Dictionary<string, GameData> Games = new Dictionary<string, GameData>();
    public readonly Dictionary<int, string> ActiveLogins = new Dictionary<int, string>();
    public readonly Dictionary<int, string> PlayerLobbies = new Dictionary<int, string>();

    public Dictionary<string, string> Credentials;
    private Main main;
    private ServerLogic serverLogic;

    public override void _Ready()
    {
        base._Ready();
        this.Credentials = FileStorage.LoadCredentials() ?? new Dictionary<string, string> { { "Server", "" } };
        this.main = GetNode<Main>("/root/Main");
        this.serverLogic = new ServerLogic();
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        if (GetTree().NetworkPeer is WebSocketServer server && server.IsListening())
        {
            server.Poll();
        }
        else if (GetTree().NetworkPeer is WebSocketClient client &&
           (
           client.GetConnectionStatus() == NetworkedMultiplayerPeer.ConnectionStatus.Connected ||
           client.GetConnectionStatus() == NetworkedMultiplayerPeer.ConnectionStatus.Connecting
           ))
        {
            client.Poll();
        }

        foreach (var lobby in this.Games)
        {
            this.serverLogic.CheckTimeout(lobby.Value, delta);
        }
    }

    #region Peers connection

    public void CreateServer()
    {
        var peer = new WebSocketServer();
        peer.Listen(ConnectionPort, null, true);
        GetTree().NetworkPeer = peer;

        GetTree().Connect("network_peer_connected", this, nameof(PlayerConnected));
        GetTree().Connect("network_peer_disconnected", this, nameof(PlayerDisconnected));

        this.ActiveLogins.Clear();
        this.Lobbies.Clear();
        this.ActiveLogins[1] = "Server";
        
        LoginSuccess();
    }

    public void CreateClient(string serverAddress, string login, string password)
    {
        var peer = new WebSocketClient();
        peer.ConnectToUrl($"ws://{serverAddress}:{ConnectionPort}", null, true);
        GetTree().NetworkPeer = peer;

        GetTree().Connect("network_peer_connected", this, nameof(PlayerConnected));
        GetTree().Connect("network_peer_disconnected", this, nameof(PlayerDisconnected));
        GetTree().Connect("connected_to_server", this, nameof(PlayerConnectedToServer), new Godot.Collections.Array { login, password });
    }

    [RemoteSync]
    private void LoginOnServer(string login, string password)
    {
        var clientId = GetTree().GetRpcSenderId();
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            RpcId(clientId, nameof(IncorrectLogin));
            return;
        }

        var hash = Convert.ToBase64String(MD5.Create().ComputeHash(Encoding.Unicode.GetBytes($"{login}_{password}")));
        if (!this.Credentials.ContainsKey(login))
        {
            this.Credentials[login] = hash;
            this.ActiveLogins[clientId] = login;
            RpcId(clientId, nameof(LoginSuccess));

            FileStorage.SaveCredentials(this.Credentials);
        }
        else if (this.Credentials[login] == hash)
        {
            this.ActiveLogins[clientId] = login;
            RpcId(clientId, nameof(LoginSuccess));
        }
        else
        {
            RpcId(clientId, nameof(IncorrectLogin));
        }
    }

    [RemoteSync]
    private void LoginSuccess()
    {
        this.main.LoginSuccess();
    }

    [RemoteSync]
    private void IncorrectLogin()
    {
        GetTree().NetworkPeer = null;

        GetTree().Disconnect("network_peer_connected", this, nameof(PlayerConnected));
        GetTree().Disconnect("network_peer_disconnected", this, nameof(PlayerDisconnected));
        GetTree().Disconnect("connected_to_server", this, nameof(PlayerConnectedToServer));

        this.main.IncorrectLogin();
    }

    private void PlayerConnected(int id)
    {
        GD.Print("player connected");
    }

    private void PlayerConnectedToServer(string login, string password)
    {
        RpcId(1, nameof(LoginOnServer), login, password);
    }

    private void PlayerDisconnected(int id)
    {
        if (!ActiveLogins.ContainsKey(id))
        {
            return;
        }

        if (this.PlayerLobbies.ContainsKey(id))
        {
            var lobbyId = this.PlayerLobbies[id];
            if (Lobbies.ContainsKey(lobbyId))
            {
                SendAllPlayerLeftLobby(id);
            }

            if (Games.ContainsKey(lobbyId)){
                PlayerDisconnectedOnServer(id);
            }

            PlayerLobbies.Remove(id);
        }

        this.ActiveLogins.Remove(id);

        GD.Print("player disconnected");
    }

    #endregion

    #region Joining lobby

    public void CreateLobby()
    {
        RpcId(1, nameof(CreateLobbyOnServer));
    }

    [RemoteSync]
    private void CreateLobbyOnServer()
    {
        var lobbyId = "Lobby" + LobbyIndex;
        LobbyIndex++;
        var lobbyData = new LobbyData();
        Lobbies[lobbyId] = lobbyData;
        var creatorClientId = GetTree().GetRpcSenderId();
        lobbyData.Creator = creatorClientId;
        PlayerLobbies[creatorClientId] = lobbyId;
        RpcId(creatorClientId, nameof(CreateLobbyDone), lobbyId);
    }

    [RemoteSync]
    private void CreateLobbyDone(string lobbyId)
    {
        this.main.LobbyCreated(lobbyId);
    }

    public void JoinLobby(string lobbyId)
    {
        RpcId(1, nameof(JoinLobbyOnServer), lobbyId);
    }

    [RemoteSync]
    private void JoinLobbyOnServer(string lobbyId)
    {
        var clientId = GetTree().GetRpcSenderId();
        if (!Lobbies.ContainsKey(lobbyId))
        {
            RpcId(clientId, nameof(LobbyNotFound), lobbyId);
            return;
        }

        var lobbyData = Lobbies[lobbyId];
        PlayerLobbies[clientId] = lobbyId;
        SendAllNewPlayerJoinedLobby(clientId);
    }

    [RemoteSync]
    private void LobbyNotFound(string lobbyId)
    {
        this.main.LobbyNotFound(lobbyId);
    }

    public void AddBot()
    {
        RpcId(1, nameof(AddBotOnServer));
    }

    [RemoteSync]
    public void AddBotOnServer()
    {
        var clientId = GetTree().GetRpcSenderId();
        var lobbyId = PlayerLobbies[clientId];
        var lobbyData = Lobbies[lobbyId];
        if (lobbyData.Creator != clientId)
        {
            return;
        }

        SendAllNewPlayerJoinedLobby(BotId);
    }

    private void SendAllNewPlayerJoinedLobby(int clientId)
    {
        var lobbyId = PlayerLobbies[(clientId == BotId) ? GetTree().GetRpcSenderId() : clientId];
        var lobbyData = Lobbies[lobbyId];
        var playerName = clientId != BotId ? this.ActiveLogins[clientId] : BotName;

        if (clientId != BotId)
        {
            RpcId(clientId, nameof(YouJoinedToLobby), clientId == lobbyData.Creator, lobbyId, playerName);
        }

        foreach (var player in lobbyData.Players)
        {
            if (player.ClientId != BotId)
            {
                RpcId(player.ClientId, nameof(PlayerJoinedToLobby), playerName);
            }

            if (clientId != BotId)
            {
                RpcId(clientId, nameof(PlayerJoinedToLobby), player.PlayerName);
            }
        }

        lobbyData.Players.Add(new LobbyData.PlayerData { ClientId = clientId, PlayerName = playerName });
    }

    public void LeaveLobby()
    {
        RpcId(1, nameof(LeaveLobbyOnServer));
    }

    [RemoteSync]
    private void LeaveLobbyOnServer()
    {
        var clientId = GetTree().GetRpcSenderId();
        SendAllPlayerLeftLobby(clientId);

        var lobbyId = PlayerLobbies[clientId];
        var playerName = this.ActiveLogins[clientId];
        var lobbyData = Lobbies[lobbyId];
        if (lobbyData.Players.All(a => a.ClientId == BotId))
        {
            Lobbies.Remove(lobbyId);
        }

        PlayerLobbies.Remove(clientId);

    }

    private void SendAllPlayerLeftLobby(int clientId)
    {
        var lobbyId = PlayerLobbies[clientId];
        var playerName = this.ActiveLogins[clientId];
        var lobbyData = Lobbies[lobbyId];
        foreach (var player in lobbyData.Players)
        {
            if (player.ClientId != BotId)
            {
                RpcId(player.ClientId, nameof(PlayerLeftLobby), playerName);
            }

            if (clientId != BotId)
            {
                RpcId(clientId, nameof(PlayerLeftLobby), playerName);
            }
        }

        if (clientId != BotId)
        {
            RpcId(clientId, nameof(YouLeftLobby));
        }

        lobbyData.Players.RemoveAll(a => a.ClientId == clientId);
    }

    [RemoteSync]
    private void PlayerLeftLobby(string playerName)
    {
        this.main.PlayerLeftLobby(playerName);
    }

    [RemoteSync]
    private void YouLeftLobby()
    {
        this.main.YouLeftLobby();
    }

    [RemoteSync]
    private void PlayerJoinedToLobby(string playerName)
    {
        this.main.PlayerJoinedToLobby(playerName);
    }

    [RemoteSync]
    private void YouJoinedToLobby(bool creator, string lobbyId, string playerName)
    {
        this.main.YouJoinedToLobby(creator, lobbyId, playerName);
    }

    #endregion

    #region Start game

    public void StartGame(bool fullMapVisible, bool turnTimeoutEnaled, float turnTimeoutValue)
    {
        RpcId(1, nameof(StartGameOnServer), fullMapVisible, turnTimeoutEnaled, turnTimeoutValue);
    }

    [RemoteSync]
    public void StartGameOnServer(bool fullMapVisible, bool turnTimeoutEnaled, float turnTimeoutValue)
    {
        var clientId = GetTree().GetRpcSenderId();
        var lobbyId = PlayerLobbies[clientId];
        var lobbyData = Lobbies[lobbyId];
        if (lobbyData.Creator != clientId)
        {
            return;
        }

        var game = this.serverLogic.Start(new ServerConfiguration
        {
            FullMapVisible = fullMapVisible,
            TurnTimeout = turnTimeoutEnaled ? (float?)turnTimeoutValue : null,
            PlayersCount = lobbyData.Players.Count,
        });

        Lobbies.Remove(lobbyId);
        Games.Add(lobbyId, game);

        var botNumber = 0;
        foreach (var player in lobbyData.Players)
        {
            if (player.ClientId == BotId)
            {
                botNumber--;

                var bot = new Bot();
                bot.NewGame(game, botNumber);
            }
            else
            {
                RpcId(player.ClientId, nameof(GameStarted));
            }
        }
    }

    [RemoteSync]
    private void GameStarted()
    {
        this.main.GameStarted();
    }

    #endregion

    #region Game steps

    public void ConnectToServer(TransferConnectData data)
    {
        RpcId(1, nameof(ConnectToServerOnServer), JsonConvert.SerializeObject(data));
    }

    [RemoteSync]
    private void ConnectToServerOnServer(string data)
    {
        TransferConnectData connectData = JsonConvert.DeserializeObject<TransferConnectData>(data);
        var clientId = GetTree().GetRpcSenderId();
        var lobbyId = PlayerLobbies[clientId];
        this.serverLogic.Connect(Games[lobbyId], clientId, connectData,
            (initData) => { RpcId(clientId, nameof(Initialize), JsonConvert.SerializeObject(initData)); },
            (turnData) => { RpcId(clientId, nameof(TurnDone), JsonConvert.SerializeObject(turnData)); });
    }

    [RemoteSync]
    private void Initialize(string data)
    {
        TransferInitialData initialData = JsonConvert.DeserializeObject<TransferInitialData>(data);
        this.main.Initialize(initialData);
    }

    public void PlayerMoved(TransferTurnDoneData data)
    {
        RpcId(1, nameof(PlayerMovedOnServer), JsonConvert.SerializeObject(data));
    }

    [RemoteSync]
    private void PlayerMovedOnServer(string data)
    {
        TransferTurnDoneData turnData = JsonConvert.DeserializeObject<TransferTurnDoneData>(data);
        var clientId = GetTree().GetRpcSenderId();
        var lobbyId = PlayerLobbies[clientId];
        
        this.serverLogic.PlayerMove(Games[lobbyId], clientId, turnData);

        foreach (var playerGameOver in Games[lobbyId].PlayersGameOver)
        {
            if (PlayerLobbies.ContainsKey(playerGameOver.Key) && PlayerLobbies[playerGameOver.Key] == lobbyId)
            {
                PlayerLobbies.Remove(playerGameOver.Key);
            }
        }

        if (Games[lobbyId].Players.Count == 0)
        {
            Games.Remove(lobbyId);
        }
    }

    [RemoteSync]
    private void TurnDone(string data)
    {
        TransferTurnData turnData = JsonConvert.DeserializeObject<TransferTurnData>(data);
        this.main.TurnDone(turnData);
    }

    private void PlayerDisconnectedOnServer(int clientId)
    {
        var lobbyId = PlayerLobbies[clientId];

        this.serverLogic.PlayerDisconnect(Games[lobbyId], clientId);
    }

    #endregion
}
