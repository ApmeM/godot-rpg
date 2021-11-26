using Godot;
using IsometricGame.Logic;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.Models;
using IsometricGame.Logic.Utils;
using IsometricGame.Repository;
using Newtonsoft.Json;

public class Communicator : Node
{
    private const int ConnectionPort = 12345;
    private const int ClientConnectionTimeout = 5;

    private GamesRepository gamesRepository;
    private AccountRepository accountRepository;
    private ServerLogic serverLogic;
    private Main main;
    private float clientConnectingTime = 0;

    public override void _Ready()
    {
        base._Ready();
        this.gamesRepository = DependencyInjector.gamesRepository;
        this.accountRepository = DependencyInjector.accountRepository;
        this.serverLogic = DependencyInjector.serverLogic;
        this.main = GetNode<Main>("/root/Main");

        GetTree().Connect("network_peer_connected", this, nameof(PlayerConnected));
        GetTree().Connect("network_peer_disconnected", this, nameof(PlayerDisconnected));
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
            if(client.GetConnectionStatus() == NetworkedMultiplayerPeer.ConnectionStatus.Connecting)
            {
                clientConnectingTime += delta;
                if(clientConnectingTime > ClientConnectionTimeout)
                {
                    GetTree().NetworkPeer = null;
                    clientConnectingTime = 0;
                    this.IncorrectLogin();
                }
            }
            client.Poll();
        }

        this.serverLogic.ProcessTick(delta);
    }

    #region Peers connection

    public void CreateServer()
    {
        var peer = new WebSocketServer();
        peer.Listen(ConnectionPort, null, true);
        GetTree().NetworkPeer = peer;

        this.serverLogic.InitializeServer();
        
        LoginSuccess();
    }

    public void CreateClient(string serverAddress, string login, string password)
    {
        this.clientConnectingTime = 0;

        var peer = new WebSocketClient();
        peer.ConnectToUrl($"ws://{serverAddress}:{ConnectionPort}", null, true);
        GetTree().NetworkPeer = peer;

        GetTree().Disconnect("connected_to_server", this, nameof(PlayerConnectedToServer));
        GetTree().Connect("connected_to_server", this, nameof(PlayerConnectedToServer), new Godot.Collections.Array { login, password });
    }

    [RemoteSync]
    private void LoginOnServer(string login, string password)
    {
        var clientId = GetTree().GetRpcSenderId();
        bool isSuccess = this.serverLogic.Login(clientId, login, password);
        if (!isSuccess)
        {
            RpcId(clientId, nameof(IncorrectLogin));
        }
        else
        {
            RpcId(clientId, nameof(LoginSuccess));
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

        this.main.IncorrectLogin();
    }

    private void PlayerConnected(int id)
    {
    }

    private void PlayerConnectedToServer(string login, string password)
    {
        RpcId(1, nameof(LoginOnServer), login, password);
    }

    private void PlayerDisconnected(int id)
    {
        SendAllPlayerLeftLobby(id);
        this.serverLogic.Logout(id);
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
        var creatorClientId = GetTree().GetRpcSenderId();
        var lobbyId = this.serverLogic.CreateLobby(creatorClientId);
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
        var newPlayer = this.serverLogic.JoinLobby(clientId, lobbyId);
        if (newPlayer == null)
        {
            RpcId(clientId, nameof(LobbyNotFound), lobbyId);
            return;
        }

        var lobby = this.gamesRepository.FindForClientLobby(clientId);
        SendAllNewPlayerJoinedLobby(lobby, newPlayer);
    }

    [RemoteSync]
    private void LobbyNotFound(string lobbyId)
    {
        this.main.LobbyNotFound(lobbyId);
    }

    public void AddBot(Bot bot)
    {
        RpcId(1, nameof(AddBotOnServer), bot);
    }

    [RemoteSync]
    private void AddBotOnServer(Bot bot)
    {
        var clientId = GetTree().GetRpcSenderId();
        var newPlayer = this.serverLogic.AddBot(clientId, bot);
        if (newPlayer == null)
        {
            return;
        }

        var lobby = this.gamesRepository.FindForClientLobby(clientId);
        SendAllNewPlayerJoinedLobby(lobby, newPlayer);
    }

    private void SendAllNewPlayerJoinedLobby(LobbyData lobby, LobbyData.PlayerData newPlayer)
    {
        var clientId = newPlayer.ClientId;
        var playerName = newPlayer.PlayerName;
        if (clientId > 0)
        {
            RpcId(clientId, nameof(YouJoinedToLobby), clientId == lobby.Creator, lobby.Id, playerName);
            RpcId(clientId, nameof(ConfigSynced), lobby.ServerConfiguration.FullMapVisible, lobby.ServerConfiguration.TurnTimeout != null, lobby.ServerConfiguration.TurnTimeout ?? ServerConfiguration.DefaultTurnTimeout, (int)lobby.ServerConfiguration.MapType);
        }

        foreach (var player in lobby.Players)
        {
            if (player.ClientId == clientId)
            {
                continue;
            }

            if (player.ClientId > 0)
            {
                RpcId(player.ClientId, nameof(PlayerJoinedToLobby), playerName);
            }

            if (clientId > 0)
            {
                RpcId(clientId, nameof(PlayerJoinedToLobby), player.PlayerName);
            }
        }
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
        this.serverLogic.LeaveLobby(clientId);
    }

    private void SendAllPlayerLeftLobby(int clientId)
    {
        var lobby = this.gamesRepository.FindForClientLobby(clientId);
        var playerName = this.accountRepository.FindForClientActiveLogin(clientId);

        if (lobby == null)
        {
            return;
        }

        foreach (var player in lobby.Players)
        {
            if (player.ClientId > 0)
            {
                RpcId(player.ClientId, nameof(PlayerLeftLobby), playerName);
            }

            if (clientId > 0)
            {
                RpcId(clientId, nameof(PlayerLeftLobby), playerName);
            }
        }

        if (clientId > 0)
        {
            RpcId(clientId, nameof(YouLeftLobby));
        }
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

    public void SyncConfig(bool fullMapVisible, bool turnTimeoutEnaled, float turnTimeoutValue, MapGeneratingType mapType)
    {
        RpcId(1, nameof(SyncConfigOnServer), fullMapVisible, turnTimeoutEnaled, turnTimeoutValue, (int)mapType);
    }


    [RemoteSync]
    private void SyncConfigOnServer(bool fullMapVisible, bool turnTimeoutEnaled, float turnTimeoutValue, int mapType)
    {
        var clientId = GetTree().GetRpcSenderId();

        if (!this.serverLogic.UpdateConfig(clientId, fullMapVisible, turnTimeoutEnaled, turnTimeoutValue, mapType))
        {
            return;
        }

        SendAllNewConfig(clientId);
    }

    private void SendAllNewConfig(int clientId)
    {
        var lobbyData = this.gamesRepository.FindForClientLobby(clientId);
        foreach (var player in lobbyData.Players)
        {
            if (player.ClientId < 0)
            {
                continue;
            }

            RpcId(player.ClientId, nameof(ConfigSynced), lobbyData.ServerConfiguration.FullMapVisible, lobbyData.ServerConfiguration.TurnTimeout != null, lobbyData.ServerConfiguration.TurnTimeout ?? ServerConfiguration.DefaultTurnTimeout, (int)lobbyData.ServerConfiguration.MapType);
        }
    }

    [RemoteSync]
    private void ConfigSynced(bool fullMapVisible, bool turnTimeoutEnaled, float turnTimeoutValue, int mapType)
    {
        this.main.ConfigSynced(fullMapVisible, turnTimeoutEnaled, turnTimeoutValue, (MapGeneratingType)mapType);
    }

    #endregion

    #region Start game

    public void StartGame()
    {
        RpcId(1, nameof(StartGameOnServer));
    }

    [RemoteSync]
    public void StartGameOnServer()
    {
        var clientId = GetTree().GetRpcSenderId();
        var game = this.serverLogic.StartGame(clientId);
        if (game == null)
        {
            return;
        }

        foreach (var player in game.Players)
        {
            if (player.Value.PlayerId < 0)
            {
                continue;
            }

            RpcId(player.Key, nameof(GameStarted));
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
        this.serverLogic.ConnectToGame(clientId, connectData, 
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
        this.serverLogic.PlayerMove(clientId, turnData);
    }

    [RemoteSync]
    private void TurnDone(string data)
    {
        TransferTurnData turnData = JsonConvert.DeserializeObject<TransferTurnData>(data);
        this.main.TurnDone(turnData);
    }

    #endregion
}
