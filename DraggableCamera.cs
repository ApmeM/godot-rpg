using Godot;

public class DraggableCamera : Camera2D
{
	private bool drag = false;
	private Vector2 initPosMouse = Vector2.Zero;

	public override void _Process(float delta)
	{
		base._Process(delta);

		// Input handler, listen for ESC to exit app
		if (Input.IsMouseButtonPressed(1))
		{
			if (drag)
			{
				var mousePos = this.GetViewport().GetMousePosition();
				this.GlobalPosition += (initPosMouse - mousePos) * this.Zoom;
				this.initPosMouse = mousePos;
			}
			else
			{
				if (initPosMouse == Vector2.Zero)
				{
					this.initPosMouse = this.GetViewport().GetMousePosition();
				}

				this.drag = this.initPosMouse != this.GetViewport().GetMousePosition();
			}
		}
		else
		{
			this.drag = false;
			this.initPosMouse = Vector2.Zero;
		}
	}
}
