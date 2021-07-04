using Godot;
using IsometricGame.Logic;
using System.Collections.Generic;

public class Lobby : Container
{
	private bool? isServer;
	private List<TransferConnectData> teams;
	private readonly Dictionary<int, Label> otherNames = new Dictionary<int, Label>();

	[Signal]
	public delegate void StartGameEvent(int selectedTeam);

	public override void _Ready()
	{
		base._Ready();
		this.GetNode<Button>("VBoxContainer/HBoxContainer/StartButton").Connect("pressed", this, nameof(OnStartButtonPressed));
		this.GetNode<OptionButton>("VBoxContainer/TeamSelector").Connect("item_selected", this, nameof(TeamSelectorItemSelected));
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
		}else if (GetTree().NetworkPeer is WebSocketClient client && 
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

	public void Start(bool isServer)
	{
		this.isServer = isServer;
		this.teams = TransferConnectData.Load();
		var dropDown = this.GetNode<OptionButton>("VBoxContainer/TeamSelector");
		dropDown.Clear();
		foreach(var team in this.teams)
		{
			dropDown.AddItem(team.TeamName);
		}
	}

	private void PlayerConnected(int id)
	{
		var team = this.teams[this.GetNode<OptionButton>("VBoxContainer/TeamSelector").Selected];
		RpcId(id, nameof(RegisterPlayer), team.TeamName);
	}

	private void PlayerDisconnected(int id)
	{
		otherNames[id].QueueFree();
		otherNames.Remove(id);
	}

	[RemoteSync]
	private void StartGame()
	{
		EmitSignal(nameof(StartGameEvent), this.GetNode<OptionButton>("VBoxContainer/TeamSelector").Selected);
	}

	[Remote]
	private void RegisterPlayer(string playerData)
	{
		var otherClientId = GetTree().GetRpcSenderId();
		if (!this.otherNames.ContainsKey(otherClientId))
		{
			otherNames[otherClientId] = new Label();
			this.GetNode<VBoxContainer>("VBoxContainer/VBoxContainer").AddChild(otherNames[otherClientId]);
		}
		
		var label = this.otherNames[otherClientId];
		label.Text = playerData;
	}
}
