using Godot;

namespace IsometricGame.Controllers
{
    public class MouseController : IController
    {
        private bool mouseButtonPressed = false;
        private Vector2 mousePositionOnPress;

        public Vector2 GetNewTarget(TileMap map)
        {
            if (!Input.IsMouseButtonPressed(1))
            {
                if (mouseButtonPressed)
                {
                    mouseButtonPressed = Input.IsMouseButtonPressed(1);
                    if (mousePositionOnPress == map.GetViewport().GetMousePosition())
                    {
                        var trollPosition = map.GetGlobalMousePosition();
                        return map.WorldToMap(trollPosition);
                    }
                }
            }
            else
            {
                if (!mouseButtonPressed)
                {
                    mouseButtonPressed = Input.IsMouseButtonPressed(1);
                    mousePositionOnPress = map.GetViewport().GetMousePosition();
                }
            }

            return Vector2.Zero;
        }
    }
}
