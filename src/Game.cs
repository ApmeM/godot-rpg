using Godot;
using IsometricGame.Logic;

public class Game : Node2D
{
    public void NewGame()
    {
        if (GetTree().IsNetworkServer())
        {
            var server = new Server();
            server.Start(new ServerConfiguration
            {
                FullMapVisible = true
            });

            //GetNode<Bot>("Bot").NewGame(server);
            GetNode<Dungeon>("Dungeon").NewGame(server);
        }
        else
        {
            GetNode<Dungeon>("Dungeon").NewGame(null);
        }
    }
}
