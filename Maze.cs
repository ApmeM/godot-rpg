using Godot;
using IsometricGame.Logic.Models;

public class Maze : TileMap
{

	public override void _Ready()
	{
		var tileMapFloor = GetNode<TileMap>("Floor");
		this.Clear();
		tileMapFloor.Clear();
	}

	public void NewVisibleMap(MapTile[,] maze)
	{
		var tileMapFloor = GetNode<TileMap>("Floor");
		
		for (var x = 0; x < maze.GetLength(0); x++)
			for (var y = 0; y < maze.GetLength(1); y++)
			{
				switch (maze[x, y])
				{
					case MapTile.Path:
						{
							tileMapFloor.SetCellv(new Vector2(x, y), 0);
							break;
						}
					case MapTile.Wall:
						{
							this.SetCellv(new Vector2(x, y), 2);
							break;
						}
					default:
						{
							break;
						}
				}
			}
	}
}
