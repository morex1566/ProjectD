using UnityEngine;

namespace TRPG.Runtime
{
    public class AStarNode
    {
        public int X;
        public int Y;
        public bool IsWalkable;

        public int GCost;
        public int HCost;
        public int FCost => GCost + HCost;

        public AStarNode Parent;

        public AStarNode(int x, int y, bool isWalkable)
        {
            X = x;
            Y = y;
            IsWalkable = isWalkable;
        }
    }
}
