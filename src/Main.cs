using Godot;
using IsometricGame.Logic;

public class Main : Node
{
	private Game game;
	private Menu menu;
	private Lobby lobby;

	public override void _Ready()
	{
		base._Ready();

		this.game = GetNode<Game>("Game");
		this.menu = GetNode<Menu>("Menu");
		this.lobby = GetNode<Lobby>("Lobby");

		RemoveChild(this.game);
		RemoveChild(this.lobby);

		this.menu.Connect(nameof(Menu.CreateLobby), this, nameof(CreateLobby));
		this.menu.Connect(nameof(Menu.JoinLobby), this, nameof(JoinLobby));
		this.lobby.Connect(nameof(Lobby.StartGameEvent), this, nameof(StartGame));
	}

	public void GameOver()
	{
		RemoveChild(this.game);
		AddChild(this.menu);
		this.menu.GameOver();
	}

	public void CreateLobby()
	{
		this.lobby.Start(true);
		RemoveChild(this.menu);
		AddChild(this.lobby);
	}
	public void JoinLobby()
	{
		this.lobby.Start(false);
		RemoveChild(this.menu);
		AddChild(this.lobby);
	}

	public void StartGame(int selectedTeam, int botsCount, ServerConfiguration serverConfiguration)
	{
		RemoveChild(this.lobby);
		AddChild(this.game);

		this.game.NewGame(selectedTeam, botsCount, serverConfiguration);
	}
}
