using Godot;
using IsometricGame.Logic;

public class TimeoutChecker : Node
{
	public override void _Process(float delta)
	{
		base._Process(delta);
		foreach (var lobby in Server.Lobbies)
		{
			lobby.Value.Server?.CheckTimeout(delta);
		}
	}
}
