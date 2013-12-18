using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace itp380
{
    public class Node
    {
        public List<Node> AdjacentNodes = new List<Node>();
        public Vector2 GridPos;
        public Node Parent = null;
        public float CostSoFar;
        public float HeuristicCostToGoal;

        public Vector3 GridPos3
        {
            get { return new Vector3(GridPos.X, GridPos.Y, 0); }
        }

        public Node(Vector2 gridPos, float heuristicCostToGoal)
        {
            GridPos = gridPos;
            HeuristicCostToGoal = heuristicCostToGoal;
        }

        public float EstTotalCost()
        {
            return CostSoFar + HeuristicCostToGoal;
        }
    }
}
