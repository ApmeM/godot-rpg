using Godot;

namespace IsometricGame.Controllers
{
    public interface IController
    {
        Vector2 GetNewTarget(TileMap map);
    }
}
