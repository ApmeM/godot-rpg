using Godot;
using IsometricGame.Logic;
using System.Collections.Generic;

public class Lobby : Container
{
	private bool? isServer;
	private List<TransferConnectData> teams;
	private readonly Dictionary<int, Label> otherNames = new Dictionary<int, Label>();

	[Signal]
	public delegate void StartGameEvent(int selectedTeam, int botsCount, ServerConfiguration serverConfiguration);

	private int botsCount = 0;
	private int playersCount = 1;

	public override void _Ready()
	{
		base._Ready();
		this.GetNode<Button>("VBoxContainer/HBoxContainer/StartButton").Connect("pressed", this, nameof(OnStartButtonPressed));
		this.GetNode<Button>("VBoxContainer/HBoxContainer/AddBotButton").Connect("pressed", this, nameof(OnAddBotButtonPressed));
		this.GetNode<OptionButton>("VBoxContainer/HBoxContainer2/VBoxContainer/TeamSelector").Connect("item_selected", this, nameof(TeamSelectorItemSelected));
		GetTree().Connect("network_peer_connected", this, nameof(PlayerConnected));
		GetTree().Connect("network_peer_disconnected", this, nameof(PlayerDisconnected));
		//GetTree().Connect("connected_to_server", this, "ConnectedOk");
		//GetTree().Connect("connection_failed", this, "ConnectedFail");
		//GetTree().Connect("server_disconnected", this, "ServerDisconnected");
	}

	public override void _EnterTree()
	{
		base._EnterTree();
		if (isServer.HasValue)
		{
			this.GetNode<Button>("VBoxContainer/HBoxContainer/StartButton").Visible = isServer.Value;
			this.GetNode<Button>("VBoxContainer/HBoxContainer/AddBotButton").Visible = isServer.Value;

			if (isServer.Value)
			{
				var peer = new WebSocketServer();
				peer.Listen(12345, null, true);
				GetTree().NetworkPeer = peer;
			}
			else
			{
				var peer = new WebSocketClient();
				peer.ConnectToUrl("ws://localhost:12345", null, true);
				GetTree().NetworkPeer = peer;
			}
		}
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
	}

	private void TeamSelectorItemSelected(int id)
	{
		var team = this.teams[id];
		Rpc(nameof(RegisterPlayer), team.TeamName);
	}

	public void OnStartButtonPressed()
	{
		Rpc(nameof(StartGame));
	}

	[RemoteSync]
	private void StartGame()
	{
		var container = this.GetNode<Container>("VBoxContainer/HBoxContainer2/VBoxContainer2");
		var serverConfiguration = new ServerConfiguration
		{
			FullMapVisible = container.GetNode<CheckBox>("ViewFullMap").Pressed,
			PlayersCount = botsCount + playersCount,
		};

		EmitSignal(nameof(StartGameEvent), this.GetNode<OptionButton>("VBoxContainer/HBoxContainer2/VBoxContainer/TeamSelector").Selected, botsCount, serverConfiguration);
	}

	public void OnAddBotButtonPressed()
	{
		Rpc(nameof(AddBot));
		botsCount++;
	}

	[RemoteSync]
	public void AddBot()
	{
		var container = this.GetNode<Container>("VBoxContainer/HBoxContainer2/VBoxContainer/VBoxContainer");
		container.AddChild(new Label { Text = "Bot" });
	}

	public void Start(bool isServer)
	{
		this.isServer = isServer;
		this.teams = TransferConnectData.Load();
		var dropDown = this.GetNode<OptionButton>("VBoxContainer/HBoxContainer2/VBoxContainer/TeamSelector");
		dropDown.Clear();
		foreach(var team in this.teams)
		{
			dropDown.AddItem(team.TeamName);
		}
	}

	private void PlayerConnected(int id)
	{
		var team = this.teams[this.GetNode<OptionButton>("VBoxContainer/HBoxContainer2/VBoxContainer/TeamSelector").Selected];
		RpcId(id, nameof(RegisterPlayer), team.TeamName);
		playersCount++;
	}

	private void PlayerDisconnected(int id)
	{
		otherNames[id].QueueFree();
		otherNames.Remove(id);
		playersCount--;
	}

	[Remote]
	private void RegisterPlayer(string playerData)
	{
		var otherClientId = GetTree().GetRpcSenderId();
		if (!this.otherNames.ContainsKey(otherClientId))
		{
			otherNames[otherClientId] = new Label();
			this.GetNode<VBoxContainer>("VBoxContainer/HBoxContainer2/VBoxContainer/VBoxContainer").AddChild(otherNames[otherClientId]);
		}
		
		var label = this.otherNames[otherClientId];
		label.Text = playerData;
	}
}
