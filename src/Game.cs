using Godot;
using IsometricGame.Logic;

public class Game : Node2D
{
    public void NewGame(int selectedTeam)
    {
        if (GetTree().IsNetworkServer())
        {
            var server = new Server();
            server.Start(new ServerConfiguration
            {
                FullMapVisible = true
            });

            //GetNode<Bot>("Bot").NewGame(server);
            GetNode<Dungeon>("Dungeon").NewGame(selectedTeam, server);
        }
        else
        {
            GetNode<Dungeon>("Dungeon").NewGame(selectedTeam, null);
        }
    }
}
