using Godot;
using IsometricGame.Logic;
using IsometricGame.Logic.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

public class Communicator : Node
{
    public Dictionary<string, LobbyData> Lobbies = new Dictionary<string, LobbyData>();

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
    }

    public void CreateClient(string serverAddress)
    {
        var peer = new WebSocketClient();
        peer.ConnectToUrl($"ws://{serverAddress}:12345", null, true);
        GetTree().NetworkPeer = peer;

        GetTree().Connect("network_peer_connected", this, nameof(PlayerConnected));
        GetTree().Connect("network_peer_disconnected", this, nameof(PlayerDisconnected));
    }

    private void PlayerConnected(int id)
    {
        GD.Print("player connected");
    }

    private void PlayerDisconnected(int id)
    {
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
        lobbyData.Creator = creatorClientId;
        lobbyData.Players.Add(creatorClientId);
        RpcId(creatorClientId, nameof(PlayerJoinedToLobby), lobbyId, creatorClientId.ToString());
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
        foreach (var playerClientId in lobbyData.Players)
        {
            if (playerClientId == -1)
            {
                RpcId(clientId, nameof(PlayerJoinedToLobby), lobbyId, "Bot");
            }
            else
            {
                RpcId(playerClientId, nameof(PlayerJoinedToLobby), lobbyId, clientId.ToString());
                RpcId(clientId, nameof(PlayerJoinedToLobby), lobbyId, playerClientId.ToString());
            }
        }

        lobbyData.Players.Add(clientId);
        RpcId(clientId, nameof(PlayerJoinedToLobby), lobbyId, clientId.ToString());
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

        lobbyData.Players.Add(-1);

        foreach (var playerClientId in lobbyData.Players)
        {
            if (playerClientId == -1)
            {
                continue;
            }

            RpcId(playerClientId, nameof(PlayerJoinedToLobby), lobbyId, "Bot");
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
        foreach (var playerClientId in lobbyData.Players)
        {
            if (playerClientId == -1)
            {
                botNumber--;

                var bot = new Bot();
                bot.NewGame(lobbyData.Server, botNumber);
            }
            else
            {
                RpcId(playerClientId, nameof(GameStarted), lobbyId);
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