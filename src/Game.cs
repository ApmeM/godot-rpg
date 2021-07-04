using Godot;
using IsometricGame.Logic;

public class Game : Node2D
{
    [Export]
    public PackedScene Bot;

    public void NewGame(int selectedTeam)
    {
        if (GetTree().IsNetworkServer())
        {
            var server = new Server();
            server.Start(new ServerConfiguration
            {
                FullMapVisible = true
            });

            var bot = (Bot)Bot.Instance();
            this.AddChild(bot);
            bot.NewGame(server);
            GetNode<Dungeon>("Dungeon").NewGame(selectedTeam, server);
        }
        else
        {
            GetNode<Dungeon>("Dungeon").NewGame(selectedTeam, null);
        }
    }
}
