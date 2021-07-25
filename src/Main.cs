using Godot;
using IsometricGame.Logic;

public class Main : Node
{
	private Dungeon dungeon;
	private Menu menu;
	private Lobby lobby;

	private int selectedTeam;

	public override void _Ready()
	{
		base._Ready();

		this.dungeon = GetNode<Dungeon>("Dungeon");
		this.menu = GetNode<Menu>("Menu");
		this.lobby = GetNode<Lobby>("Lobby");

		RemoveChild(this.dungeon);
		RemoveChild(this.lobby);

		this.menu.Connect(nameof(Menu.CreateLobby), this, nameof(CreateLobby));
		this.menu.Connect(nameof(Menu.JoinLobby), this, nameof(JoinLobby));
		this.lobby.Connect(nameof(Lobby.StartGameClientEvent), this, nameof(StartGameClient));
	}

	public void GameOver()
	{
		RemoveChild(this.dungeon);
		AddChild(this.menu);
		this.menu.GameOver();
	}

	public void CreateLobby(int selectedTeam)
	{
		this.selectedTeam = selectedTeam;
		RemoveChild(this.menu);
		AddChild(this.lobby);
		this.lobby.Create();
	}

	public void JoinLobby(int selectedTeam, string lobbyId)
	{
		this.selectedTeam = selectedTeam;
		RemoveChild(this.menu);
		AddChild(this.lobby);
		this.lobby.Join(lobbyId);
	}

	public void StartGameClient(string lobbyId)
	{
		RemoveChild(this.lobby);
		AddChild(this.dungeon);

		this.dungeon.NewGame(this.selectedTeam, lobbyId);
	}
}
