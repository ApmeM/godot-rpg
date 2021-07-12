using BrainAI.Pathfinding.AStar;
using Godot;
using System;
using System.Collections.Generic;

namespace IsometricGame.Logic.ScriptHelpers
{
    public class VectorGridGraph : IAstarGraph<Vector2>
    {
        public VectorGridGraph(int width, int height, bool allowDiagonalSearch = false)
        {
            this.Width = width;
            this.Height = height;
            this.dirs = allowDiagonalSearch ? CompassDirs : CardinalDirs;
        }

        public HashSet<Vector2> WeightedNodes = new HashSet<Vector2>();
        public int DefaultWeight = 1;
        public int WeightedNodeWeight = 5;

        private static readonly Vector2[] CardinalDirs = {
            new Vector2( 1, 0 ),
            new Vector2( 0, -1 ),
            new Vector2( -1, 0 ),
            new Vector2( 0, 1 ),
        };

        private static readonly Vector2[] CompassDirs = {
            new Vector2( 1, 0 ),
            new Vector2( 1, -1 ),
            new Vector2( 0, -1 ),
            new Vector2( -1, -1 ),
            new Vector2( -1, 0 ),
            new Vector2( -1, 1 ),
            new Vector2( 0, 1 ),
            new Vector2( 1, 1 ),
        };

        public HashSet<Vector2> Paths = new HashSet<Vector2>();

        public readonly int Width, Height;
        private readonly Vector2[] dirs;
        private readonly List<Vector2> neighbors = new List<Vector2>(4);

        public bool IsNodeInBounds(Vector2 node)
        {
            return 0 <= node.x && node.x < this.Width && 0 <= node.y && node.y < this.Height;
        }

        public bool IsNodePassable(Vector2 node)
        {
            return this.Paths.Contains(node);
        }

        public IEnumerable<Vector2> GetNeighbors(Vector2 node)
        {
            this.neighbors.Clear();

            foreach (var dir in this.dirs)
            {
                var next = new Vector2(node.x + dir.x, node.y + dir.y);
                if (this.IsNodeInBounds(next) && this.IsNodePassable(next))
                    this.neighbors.Add(next);
            }

            return this.neighbors;
        }

        public int Cost(Vector2 from, Vector2 to)
        {
            return this.WeightedNodes.Contains(to) ? this.WeightedNodeWeight : this.DefaultWeight;
        }

        public int Heuristic(Vector2 node, Vector2 goal)
        {
            return (int)Math.Abs(node.x - goal.x) + (int)Math.Abs(node.y - goal.y);
        }

    }
}
