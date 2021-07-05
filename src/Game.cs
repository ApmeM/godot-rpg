using Godot;
using IsometricGame.Logic;

public class Game : Node2D
{
    [Export]
    public PackedScene Bot;

    public void NewGame(int selectedTeam, int botsCount, ServerConfiguration serverConfiguration)
    {
        if (GetTree().IsNetworkServer())
        {
            var server = new Server();
            server.Start(serverConfiguration);

            for (var i = 0; i < botsCount; i++)
            {
                var bot = (Bot)Bot.Instance();
                this.AddChild(bot);
                bot.NewGame(server, -i - 1);
            }
            GetNode<Dungeon>("Dungeon").NewGame(selectedTeam, server);
        }
        else
        {
            GetNode<Dungeon>("Dungeon").NewGame(selectedTeam, null);
        }
    }
}
