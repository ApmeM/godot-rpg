using BrainAI.Pathfinding.AStar;
using BrainAI.Pathfinding.BreadthFirst;
using FateRandom;
using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.ScriptHelpers;
using IsometricGame.Presentation;
using System;
using System.Collections.Generic;
using System.Linq;

[SceneReference("Maze.tscn")]
public partial class Maze : TileMap
{
    public enum HighliteType
    {
        Move = 7,
        AttackRadius = 6,
        AttackDistance = 5,
        HighlitedMove = 8
    }

    private readonly List<Vector2> lastHighlitedCells = new List<Vector2>();
    private readonly Dictionary<HighliteType, List<Vector2>> highlitedCells;

    public MapGraphData astarMove;
    public MapGraphData astarFly;
    private int? attackRadius;

    [Signal]
    public delegate void CellSelected(Vector2 cell, Vector2 cellPosition, bool moveAvailable);

    public Maze()
    {
        this.highlitedCells = Enum.GetValues(typeof(HighliteType)).Cast<HighliteType>().ToDictionary(a => a, a => new List<Vector2>());
    }

    public override void _Ready()
    {
        base._Ready();
        this.FillMembers();
        this.Clear();
        floor.Clear();
        fog.Clear();
    }

    public void NewVisibleMap(MapTile[,] maze)
    {
        for (var x = 0; x < maze.GetLength(0); x++)
            for (var y = 0; y < maze.GetLength(1); y++)
            {
                var position = new Vector2(x, y);
                switch (maze[x, y])
                {
                    case MapTile.Path:
                        {
                            astarMove.Paths.Add(position);
                            astarFly.Paths.Add(position);
                            if (floor.GetCellv(position) == -1)
                            {
                                floor.SetCellv(position, Fate.GlobalFate.Chance(90) ? 1 : 0);
                            }

                            fog.SetCellv(position, -1);
                            break;
                        }
                    case MapTile.Wall:
                        {
                            this.SetCellv(position, 2);
                            fog.SetCellv(position, -1);
                            break;
                        }
                    case MapTile.Pit:
                        {
                            astarFly.Paths.Add(position);
                            fog.SetCellv(position, -1);
                            break;
                        }
                    default:
                        {
                            fog.SetCellv(position, 9);
                            break;
                        }
                }
            }
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

        var mouse = floor.GetGlobalMousePosition();
        var cell = floor.WorldToMap(mouse);

        BeginHighliting(HighliteType.HighlitedMove, this.attackRadius);
        HighlitePoint(cell);
        EndHighliting();

        BeginHighliting(HighliteType.AttackRadius, this.attackRadius);
        if (attackRadius != null && highlitedCells[HighliteType.AttackDistance].Contains(cell))
        {
            HighliteRadius(cell, attackRadius.Value);
        }
        EndHighliting();
    }

    public void Initialize(int mapWidth, int mapHeight)
    {
        astarMove = new MapGraphData(mapWidth, mapHeight);
        astarFly = new MapGraphData(mapWidth, mapHeight);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._Input(@event);

        if (@event is InputEventScreenTouch eventMouseButton && eventMouseButton.Pressed)
        {
            var position = this.GetGlobalMousePosition(); // eventMouseButton.Position;
            var cell = this.WorldToMap(position);
            
            var floor = GetNode<TileMap>("Floor");
            if (floor.GetCellv(cell) >= 0)
            {
                var cellPosition = this.MapToWorld(cell);
                EmitSignal(nameof(CellSelected), cell, cellPosition, highlitedCells[HighliteType.Move].Contains(cell) || highlitedCells[HighliteType.AttackDistance].Contains(cell));
                GetTree().SetInputAsHandled();
            }
        }
    }

    public void RemoveHighliting()
    {
        foreach(var cells in highlitedCells.Values)
        {
            cells.Clear();
        }
        RehighliteCells();
    }

    #region HighlitingInternal

    private HighliteType? currentHighliteType;

    public void BeginHighliting(HighliteType highliteType, int? attackRadius)
    {
        if (currentHighliteType != null)
        {
            throw new Exception("Previous highliting not finished.");
        }

        this.attackRadius = attackRadius;
        this.currentHighliteType = highliteType;
        highlitedCells[highliteType].Clear();
    }

    public void HighliteRadius(Vector2 fromPoint, int highliteRadius)
    {
        BreadthFirstPathfinder.Search(this.astarFly, fromPoint, highliteRadius, out var visited);

        foreach (var cell in visited.Keys)
        {
            HighlitePoint(cell);
        }
    }

    public void HighlitePoint(Vector2 fromPoint)
    {
        EnsureHighliting();
        highlitedCells[this.currentHighliteType.Value].Add(fromPoint);
    }

    public void EndHighliting()
    {
        EnsureHighliting();
        RehighliteCells();
        this.currentHighliteType = null;
    }

    private void EnsureHighliting()
    {
        if (this.currentHighliteType == null)
        {
            throw new Exception("Highliting not started.");
        }
    }

    private void RehighliteCells()
    {
        foreach (var cell in lastHighlitedCells)
        {
            if (astarMove.Paths.Contains(cell))
            {
                floor.SetCellv(cell, Fate.GlobalFate.Chance(90) ? 1 : 0);
            }
            else
            {
                floor.SetCellv(cell, -1);
            }
        }

        lastHighlitedCells.Clear();

        foreach (var highlitedCell in highlitedCells)
        {
            foreach (var cell in highlitedCell.Value)
            {
                floor.SetCellv(cell, (int)highlitedCell.Key);
            }
            lastHighlitedCells.AddRange(highlitedCell.Value);

        }
    }

    #endregion


    public Vector2? GetMotion(Queue<Vector2> currentPath, Vector2 currentPosition, float delta, int motionSpeed)
    {
        if (currentPath.Count == 0)
        {
            return null;
        }

        var current = currentPath.Peek();
        if (Distance(current, currentPosition) < 1)
        {
            currentPath.Dequeue();
            return null;
        }

        var motion = (current - currentPosition) / delta;
        if (motion.Length() > motionSpeed)
        {
            motion = motion.Normalized() * motionSpeed;
        }

        return motion * delta;
    }

    private int Distance(Vector2 from, Vector2 to)
    {
        var vector = from - to;
        return (int)(Math.Abs(vector.x) + Math.Abs(vector.y));
    }

}
