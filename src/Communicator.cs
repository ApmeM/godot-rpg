using Godot;
using IsometricGame.Logic;
using IsometricGame.Logic.Models;
using IsometricGame.Logic.ScriptHelpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

public class Communicator : Node
{
    private const int BotId = -1;
    private const string BotName = "Bot";

    public Dictionary<string, LobbyData> Lobbies = new Dictionary<string, LobbyData>();
    public Dictionary<int, string> PlayerNames = new Dictionary<int, string>();

    public Dictionary<string, string> Credentials = new Dictionary<string, string>();

    public override void _Ready()
    {
        base._Ready();
        this.Credentials = FileStorage.LoadCredentials() ?? new Dictionary<string, string> { { "Server", "" } };
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

        foreach (var lobby in this.Lobbies)
        {
            lobby.Value.Server?.CheckTimeout(delta);
        }
    }

    #region Peers connection

    public void CreateServer()
    {
        var peer = new WebSocketServer();
        peer.Listen(12345, null, true);
        GetTree().NetworkPeer = peer;

        GetTree().Connect("network_peer_connected", this, nameof(PlayerConnected));
        GetTree().Connect("network_peer_disconnected", this, nameof(PlayerDisconnected));

        this.PlayerNames.Clear();
        this.Lobbies.Clear();
        this.PlayerNames[1] = "Server";
        
        LoginSuccess();
    }

    public void CreateClient(string serverAddress, string login, string password)
    {
        var peer = new WebSocketClient();
        peer.ConnectToUrl($"ws://{serverAddress}:12345", null, true);
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
            this.PlayerNames[clientId] = login;
            RpcId(clientId, nameof(LoginSuccess));

            FileStorage.SaveCredentials(this.Credentials);
        }
        else if (this.Credentials[login] == hash)
        {
            this.PlayerNames[clientId] = login;
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
        GetNode<Menu>("/root/Main/Menu").LoginSuccess();
    }

    [RemoteSync]
    private void IncorrectLogin()
    {
        GetTree().NetworkPeer = null;

        GetTree().Disconnect("network_peer_connected", this, nameof(PlayerConnected));
        GetTree().Disconnect("network_peer_disconnected", this, nameof(PlayerDisconnected));
        GetTree().Disconnect("connected_to_server", this, nameof(PlayerConnectedToServer));

        GetNode<Menu>("/root/Main/Menu").IncorrectLogin();
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
        if(!PlayerNames.ContainsKey(id))
        {
            return;
        }

        this.PlayerNames.Remove(id);
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
        var lobbyId = "Lobby" + Lobbies.Count;
        var lobbyData = new LobbyData();
        Lobbies[lobbyId] = lobbyData;
        var creatorClientId = GetTree().GetRpcSenderId();
        var playerName = this.PlayerNames[creatorClientId];
        lobbyData.Creator = creatorClientId;
        lobbyData.Players.Add(new LobbyData.PlayerData { ClientId = creatorClientId, PlayerName = playerName });
        RpcId(creatorClientId, nameof(PlayerJoinedToLobby), lobbyId, playerName);
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
        var playerName = this.PlayerNames[clientId];
        foreach (var player in lobbyData.Players)
        {
            if (player.ClientId == BotId)
            {
                RpcId(clientId, nameof(PlayerJoinedToLobby), lobbyId, player.PlayerName);
            }
            else
            {
                RpcId(player.ClientId, nameof(PlayerJoinedToLobby), lobbyId, playerName);
                RpcId(clientId, nameof(PlayerJoinedToLobby), lobbyId, player.PlayerName);
            }
        }

        lobbyData.Players.Add(new LobbyData.PlayerData { ClientId = clientId, PlayerName = playerName });;
        RpcId(clientId, nameof(PlayerJoinedToLobby), lobbyId, playerName);
    }
    public void AddBot(string lobbyId)
    {
        RpcId(1, nameof(AddBotOnServer), lobbyId);
    }

    [RemoteSync]
    public void AddBotOnServer(string lobbyId)
    {
        if (!Lobbies.ContainsKey(lobbyId))
        {
            return;
        }

        var lobbyData = Lobbies[lobbyId];
        var clientId = GetTree().GetRpcSenderId();
        if (lobbyData.Creator != clientId)
        {
            return;
        }

        lobbyData.Players.Add(new LobbyData.PlayerData { ClientId = BotId, PlayerName = BotName });

        foreach (var player in lobbyData.Players)
        {
            if (player.ClientId == BotId)
            {
                continue;
            }

            RpcId(player.ClientId, nameof(PlayerJoinedToLobby), lobbyId, BotName);
        }
    }

    [RemoteSync]
    private void PlayerJoinedToLobby(string lobbyId, string playerName)
    {
        GetNode<Lobby>("/root/Main/Lobby").PlayerJoinedToLobby(lobbyId, playerName);
    }

    [RemoteSync]
    private void LobbyNotFound(string lobbyId)
    {
        GetNode<Lobby>("/root/Main/Lobby").LobbyNotFound(lobbyId);
    }

    #endregion

    #region Start game

    public void StartGame(string lobbyId, bool fullMapVisible, bool turnTimeoutEnaled, float turnTimeoutValue)
    {
        RpcId(1, nameof(StartGameOnServer), lobbyId, fullMapVisible, turnTimeoutEnaled, turnTimeoutValue);
    }

    [RemoteSync]
    public void StartGameOnServer(string lobbyId, bool fullMapVisible, bool turnTimeoutEnaled, float turnTimeoutValue)
    {
        if (!Lobbies.ContainsKey(lobbyId))
        {
            return;
        }

        var lobbyData = Lobbies[lobbyId];
        var clientId = GetTree().GetRpcSenderId();
        if (lobbyData.Creator != clientId)
        {
            return;
        }

        lobbyData.Server = new ServerLogic();
        lobbyData.Server.Start(new ServerConfiguration
        {
            FullMapVisible = fullMapVisible,
            TurnTimeout = turnTimeoutEnaled ? (float?)turnTimeoutValue : null,
            PlayersCount = lobbyData.Players.Count,
        });

        var botNumber = 0;
        foreach (var player in lobbyData.Players)
        {
            if (player.ClientId == BotId)
            {
                botNumber--;

                var bot = new Bot();
                bot.NewGame(lobbyData.Server, botNumber);
            }
            else
            {
                RpcId(player.ClientId, nameof(GameStarted), lobbyId);
            }
        }
    }

    [RemoteSync]
    private void GameStarted(string lobbyId)
    {
        GetNode<Lobby>("/root/Main/Lobby").GameStarted(lobbyId);
    }

    #endregion

    #region Game steps

    public void ConnectToServer(string lobbyId, TransferConnectData data)
    {
        RpcId(1, nameof(ConnectToServerOnServer), lobbyId, JsonConvert.SerializeObject(data));
    }

    [RemoteSync]
    private void ConnectToServerOnServer(string lobbyId, string data)
    {
        TransferConnectData connectData = JsonConvert.DeserializeObject<TransferConnectData>(data);
        var clientId = GetTree().GetRpcSenderId();

        Lobbies[lobbyId].Server.Connect(clientId, connectData,
            (initData) => { RpcId(clientId, nameof(Initialize), JsonConvert.SerializeObject(initData)); },
            (turnData) => { RpcId(clientId, nameof(TurnDone), JsonConvert.SerializeObject(turnData)); });
    }

    [RemoteSync]
    private void Initialize(string data)
    {
        TransferInitialData initialData = JsonConvert.DeserializeObject<TransferInitialData>(data);
        GetNode<Dungeon>("/root/Main/Dungeon").Initialize(initialData);
    }

    public void PlayerMoved(string lobbyId, TransferTurnDoneData data)
    {
        RpcId(1, nameof(PlayerMovedOnServer), lobbyId, JsonConvert.SerializeObject(data));
    }

    [RemoteSync]
    private void PlayerMovedOnServer(string lobbyId, string data)
    {
        TransferTurnDoneData turnData = JsonConvert.DeserializeObject<TransferTurnDoneData>(data);
        var clientId = GetTree().GetRpcSenderId();

        Lobbies[lobbyId].Server.PlayerMove(clientId, turnData);
    }

    [RemoteSync]
    private void TurnDone(string data)
    {
        TransferTurnData turnData = JsonConvert.DeserializeObject<TransferTurnData>(data);
        GetNode<Dungeon>("/root/Main/Dungeon").TurnDone(turnData);
    }

    #endregion
}
