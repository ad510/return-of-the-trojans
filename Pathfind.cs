using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace itp380
{
    public class Pathfind
    {
        public class Request
        {
            public Point Start;
            public Point End;
            public Node StartNode;
            public System.Threading.Semaphore Done;
            private Pathfind m_Pathfind;

            public Request(Point Start, Point End, Pathfind m_Pathfind)
            {
                this.Start = Start;
                this.End = End;
                this.m_Pathfind = m_Pathfind;
                Done = new System.Threading.Semaphore(0, 1);
                System.Threading.Thread RequestThread = new System.Threading.Thread(Run);
                RequestThread.Start();
            }

            public void Run()
            {
                StartNode = m_Pathfind.FindPath(Start, End);
                Done.Release();
            }
        }

        public readonly int GridWidth;
        public readonly int GridHeight;
        public readonly float GridSize;
        public readonly Vector3 GridOffset;

        public bool[,] Occupied;

        public Pathfind(int GridWidth, int GridHeight, float GridSize, Vector3 GridOffset)
        {
            this.GridWidth = GridWidth;
            this.GridHeight = GridHeight;
            this.GridSize = GridSize;
            this.GridOffset = GridOffset;
            Occupied = new bool[GridWidth, GridHeight];
        }

        public void OccupyRectangle(Vector3 Min, Vector3 Max)
        {
            Point GridMin = WorldSpaceToGridFloor(Min);
            Point GridMax = WorldSpaceToGridCeiling(Max);
            for (int i = Math.Max(0, GridMin.X); i <= Math.Min(GridWidth - 1, GridMax.X); i++)
            {
                for (int j = Math.Max(0, GridMin.Y); j <= Math.Min(GridHeight - 1, GridMax.Y); j++)
                {
                    Occupied[i, j] = true;
                }
            }
        }

        public Node FindPath(Point Start, Point End)
        {
            Node[,] Nodes = new Node[GridWidth, GridHeight];
            LinkedList<Node> OpenNodes = new LinkedList<Node>();
            LinkedList<Node> ClosedNodes = new LinkedList<Node>();
            int i, j;
            // initialize end node, and add it to open list
            OpenNodes.AddLast(AddNode(End, Start, ref Nodes));
            // the heart of the A* algorithm
            // I implemented this from memory, so it looks a little different from the professor's pseudocode
            while (OpenNodes.Count > 0)
            {
                // pop first node from open list
                Node CurrentNode = OpenNodes.First.Value;
                OpenNodes.RemoveFirst();
                ClosedNodes.AddLast(CurrentNode);
                // if reached start node then exit
                if (Nodes[Start.X, Start.Y] != null)
                {
                    break;
                }
                // calculate cost to adjacent nodes, if less than current cost then add them to open list sorted by estimated total cost
                foreach (Point AdjacentNodePos in CurrentNode.AdjacentNodes)
                {
                    if (Nodes[AdjacentNodePos.X, AdjacentNodePos.Y] == null)
                    {
                        AddNode(AdjacentNodePos, Start, ref Nodes); // if node here hasn't been made yet, make it now
                    }
                    Node AdjacentNode = Nodes[AdjacentNodePos.X, AdjacentNodePos.Y];
                    float NewCostSoFar = CurrentNode.CostSoFar + Vector2.Distance(AdjacentNode.GridPos, CurrentNode.GridPos);
                    if ((AdjacentNode.Parent == null || NewCostSoFar < AdjacentNode.CostSoFar)
                        && (AdjacentNode.GridPos.X != End.X || AdjacentNode.GridPos.Y != End.Y))
                    {
                        AdjacentNode.Parent = CurrentNode;
                        AdjacentNode.CostSoFar = NewCostSoFar;
                        for (LinkedListNode<Node> node = OpenNodes.First; ; node = node.Next)
                        {
                            if (node == null)
                            {
                                OpenNodes.AddLast(AdjacentNode); // AdjacentNode has greatest estimated total cost, add to end of list
                                break;
                            }
                            if (AdjacentNode == node.Value)
                            {
                                break; // AdjacentNode is already in open list, so don't add a duplicate
                            }
                            if (AdjacentNode.EstTotalCost() <= node.Value.EstTotalCost())
                            {
                                OpenNodes.AddBefore(node, AdjacentNode);
                                break;
                            }
                        }
                    }
                }
            }
            if (Nodes[Start.X, Start.Y].Parent != null)
            {
                // do path smoothing :)
                for (Node CurrentNode = Nodes[Start.X, Start.Y]; CurrentNode != null; CurrentNode = CurrentNode.Parent)
                {
                    bool Blocked = false;
                    for (Node NextNode = CurrentNode.Parent; NextNode != null && !Blocked; NextNode = NextNode.Parent)
                    {
                        Vector2 Parallel = NextNode.GridPos - CurrentNode.GridPos;
                        Vector2 Perpendicular = new Vector2(-Parallel.Y, Parallel.X);
                        Parallel.Normalize();
                        Perpendicular.Normalize();
                        for (i = 0; i < GridWidth && !Blocked; i++)
                        {
                            for (j = 0; j < GridHeight && !Blocked; j++)
                            {
                                // if distance from smoothed path to occupied square is less than 1 square then it is blocked
                                if (Occupied[i, j] && Math.Abs(Vector2.Dot(new Vector2(i, j) - CurrentNode.GridPos, Perpendicular)) < 1
                                    && Vector2.Dot(new Vector2(i, j) - CurrentNode.GridPos, Parallel) >= 0
                                    && Vector2.Dot(new Vector2(i, j) - NextNode.GridPos, Parallel) <= 0)
                                {
                                    Blocked = true;
                                }
                            }
                        }
                        // if smoothed path is not blocked then update parent of current node
                        if (!Blocked)
                        {
                            CurrentNode.Parent = NextNode;
                        }
                    }
                }
                return Nodes[Start.X, Start.Y];
            }
            return null;
        }

        private Node AddNode(Point Pos, Point Start, ref Node[,] Nodes)
        {
            Nodes[Pos.X, Pos.Y] = new Node(new Vector2(Pos.X, Pos.Y), Vector2.Distance(new Vector2(Start.X, Start.Y), new Vector2(Pos.X, Pos.Y)));
            // find adjacent nodes
            bool left = false, right = false, above = false, below = false;
            if (Pos.X > 0 && !Occupied[Pos.X - 1, Pos.Y])
            {
                Nodes[Pos.X, Pos.Y].AdjacentNodes.Add(new Point(Pos.X - 1, Pos.Y));
                left = true;
            }
            if (Pos.X < GridWidth - 1 && !Occupied[Pos.X + 1, Pos.Y])
            {
                Nodes[Pos.X, Pos.Y].AdjacentNodes.Add(new Point(Pos.X + 1, Pos.Y));
                right = true;
            }
            if (Pos.Y > 0 && !Occupied[Pos.X, Pos.Y - 1])
            {
                Nodes[Pos.X, Pos.Y].AdjacentNodes.Add(new Point(Pos.X, Pos.Y - 1));
                above = true;
            }
            if (Pos.Y < GridHeight - 1 && !Occupied[Pos.X, Pos.Y + 1])
            {
                Nodes[Pos.X, Pos.Y].AdjacentNodes.Add(new Point(Pos.X, Pos.Y + 1));
                below = true;
            }
            if (left && above && !Occupied[Pos.X - 1, Pos.Y - 1])
            {
                Nodes[Pos.X, Pos.Y].AdjacentNodes.Add(new Point(Pos.X - 1, Pos.Y - 1));
            }
            if (right && above && !Occupied[Pos.X + 1, Pos.Y - 1])
            {
                Nodes[Pos.X, Pos.Y].AdjacentNodes.Add(new Point(Pos.X + 1, Pos.Y - 1));
            }
            if (left && below && !Occupied[Pos.X - 1, Pos.Y + 1])
            {
                Nodes[Pos.X, Pos.Y].AdjacentNodes.Add(new Point(Pos.X - 1, Pos.Y + 1));
            }
            if (right && below && !Occupied[Pos.X + 1, Pos.Y + 1])
            {
                Nodes[Pos.X, Pos.Y].AdjacentNodes.Add(new Point(Pos.X + 1, Pos.Y + 1));
            }
            return Nodes[Pos.X, Pos.Y];
        }

        public Point BoundedGridPos(Point GridPos)
        {
            Point Ret = GridPos;
            // ensure point is within grid bounds
            if (Ret.X < 0)
            {
                Ret.X = 0;
            }
            if (Ret.X >= GridWidth)
            {
                Ret.X = GridWidth - 1;
            }
            if (Ret.Y < 0)
            {
                Ret.Y = 0;
            }
            if (Ret.Y >= GridHeight)
            {
                Ret.Y = GridHeight - 1;
            }
            if (Occupied[Ret.X, Ret.Y])
            {
                // if location is occupied, find nearest unoccupied location
                Point nearestUnoccupied = new Point(-10000, -10000);
                for (int i = 0; i < GridWidth; i++)
                {
                    for (int j = 0; j < GridHeight; j++)
                    {
                        if (!Occupied[i, j] && Math.Abs(i - Ret.X) + Math.Abs(j - Ret.Y) < Math.Abs(nearestUnoccupied.X - Ret.X) + Math.Abs(nearestUnoccupied.Y - Ret.Y))
                        {
                            nearestUnoccupied = new Point(i, j);
                        }
                    }
                }
                if (nearestUnoccupied.X >= 0 && nearestUnoccupied.Y >= 0)
                {
                    Ret = nearestUnoccupied;
                }
            }
            return Ret;
        }

        public Vector3 GridToWorldSpace(Vector3 GridPos)
        {
            return GridPos * GridSize + GridOffset;
        }

        public Vector3 GridToWorldSpace(Point GridPos)
        {
            return GridToWorldSpace(new Vector3(GridPos.X, GridPos.Y, 0));
        }

        public Vector3 WorldSpaceToGrid(Vector3 WorldPos)
        {
            return (WorldPos - GridOffset) / GridSize;
        }

        public Point WorldSpaceToGridFloor(Vector3 WorldPos)
        {
            Vector3 Unrounded = WorldSpaceToGrid(WorldPos);
            return new Point((int)Math.Floor(Unrounded.X), (int)Math.Floor(Unrounded.Y));
        }

        public Point WorldSpaceToGridCeiling(Vector3 WorldPos)
        {
            Vector3 Unrounded = WorldSpaceToGrid(WorldPos);
            return new Point((int)Math.Ceiling(Unrounded.X), (int)Math.Ceiling(Unrounded.Y));
        }

        public Point WorldSpaceToGridRound(Vector3 WorldPos)
        {
            Vector3 Unrounded = WorldSpaceToGrid(WorldPos);
            return new Point((int)Math.Round(Unrounded.X), (int)Math.Round(Unrounded.Y));
        }
    }
}
