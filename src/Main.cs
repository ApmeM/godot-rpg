using Godot;

public class Main : Node
{
	private Game game;
	private Menu menu;

	public override void _Ready()
	{
		base._Ready();

		this.game = GetNode<Game>("Game");
		this.menu = GetNode<Menu>("Menu");

		RemoveChild(this.game);
		//this.game.Connect(nameof(Dungeon.GameOverEvent), this, nameof(GameOver));
		this.menu.Connect(nameof(Menu.StartGameEvent), this, nameof(NewGame));
	}

	public void GameOver(int score)
	{
		RemoveChild(this.game);
		AddChild(this.menu);
		this.menu.GameOver(score);
	}

	public void NewGame()
	{
		RemoveChild(this.menu);
		AddChild(this.game);
		this.game.NewGame();
	}
}
