using BrainAI.Pathfinding.BreadthFirst;
using FateRandom;
using Godot;
using IsometricGame.Logic.Enums;
using IsometricGame.Logic.ScriptHelpers;
using System;
using System.Collections.Generic;
using System.Linq;

public class Maze : TileMap
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

    public VectorGridGraph astar;
    private int? attackRadius;

    private TileMap floor;
    private TileMap fog;

    [Signal]
    public delegate void CellSelected(Vector2 cell, Vector2 cellPosition, bool moveAvailable);

    public Maze()
    {
        this.highlitedCells = Enum.GetValues(typeof(HighliteType)).Cast<HighliteType>().ToDictionary(a => a, a => new List<Vector2>());
    }

    public override void _Ready()
    {
        floor = GetNode<TileMap>("Floor");
        fog = GetNode<TileMap>("Fog");
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
                            astar.Paths.Add(position);
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

        if (floor.GetCellv(cell) >= 0)
        {
            BeginHighliting(HighliteType.HighlitedMove);
            HighlitePoint(cell);
            EndHighliting();
        }

        BeginHighliting(HighliteType.AttackRadius);
        if (attackRadius != null && highlitedCells[HighliteType.AttackDistance].Contains(cell))
        {
            HighliteRadius(cell, attackRadius.Value);
        }
        EndHighliting();
    }

    public void Initialize(int mapWidth, int mapHeight)
    {
        astar = new VectorGridGraph(mapWidth, mapHeight);
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

    private void RehighliteCells()
    {
        foreach (var cell in lastHighlitedCells)
        {
            floor.SetCellv(cell, Fate.GlobalFate.Chance(90) ? 1 : 0);
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

    public void RemoveHighliting()
    {
        foreach(var cells in highlitedCells.Values)
        {
            cells.Clear();
        }
        RehighliteCells();
    }

    public void HighliteAvailableAttacks(Vector2 shadowCell, int? attackDistance, int? attackRadius)
    {
        this.attackRadius = attackRadius;
        BeginHighliting(HighliteType.AttackDistance);
        HighliteRadius(shadowCell, attackDistance.Value);
        EndHighliting();
        RehighliteCells();
    }

    public void HighliteAvailableAttacks(List<Vector2> targetCells, int? attackRadius)
    {
        this.attackRadius = attackRadius;
        BeginHighliting(HighliteType.AttackDistance);
        foreach (var cell in targetCells)
        {
            HighlitePoint(cell);
        }
        EndHighliting();
        RehighliteCells();
    }

    public void HighliteAvailableMoves(Vector2 unitCell, int? moveDistance)
    {
        if (moveDistance == null)
        {
            throw new Exception("Unknown move distance to highlite. Possible reason - trying to highlite distance for enemy unit.");
        }
        
        this.attackRadius = null;
        BeginHighliting(HighliteType.Move);
        HighliteRadius(unitCell, moveDistance.Value);
        EndHighliting();
    }

    #region HighlitingInternal

    private HighliteType? currentHighliteType;

    private void BeginHighliting(HighliteType highliteType)
    {
        if (currentHighliteType != null)
        {
            throw new Exception("Previous highliting not finished.");
        }

        this.currentHighliteType = highliteType;
        highlitedCells[highliteType].Clear();
    }

    private void HighliteRadius(Vector2 fromPoint, int highliteRadius)
    {
        EnsureHighliting();
        BreadthFirstPathfinder.Search(this.astar, fromPoint, highliteRadius, out var visited);
        foreach (var cell in visited.Keys)
        {
            highlitedCells[this.currentHighliteType.Value].Add(cell);
        }
    }

    private void HighlitePoint(Vector2 fromPoint)
    {
        EnsureHighliting();
        highlitedCells[this.currentHighliteType.Value].Add(fromPoint);
    }

    private void EndHighliting()
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

    #endregion
}
