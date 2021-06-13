using Godot;
using IsometricGame.Logic;

public class Game : Node2D
{
    public void NewGame()
    {
        var server = new Server();
        server.Start(new ServerConfiguration
        {
            FullMapVisible = true
        });

        GetNode<Dungeon>("Dungeon").NewGame(server);
        GetNode<Bot>("Bot").NewGame(server);
    }
}
