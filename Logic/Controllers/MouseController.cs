using Godot;

namespace IsometricGame.Controllers
{
    public class MouseController : IController
    {
        public Vector2 GetNewTarget(TileMap map)
        {
            if (!Input.IsMouseButtonPressed(1))
            {
                return Vector2.Zero;
            }

            var trollPosition = map.GetGlobalMousePosition();
            return map.WorldToMap(trollPosition);
        }
    }
}
