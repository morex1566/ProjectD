using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// A* 탐색에서 사용하는 단일 그리드 노드 상태입니다.
    /// </summary>
    public class AStarNode
    {
        public int X;
        public int Y;
        public bool IsWalkable;

        public int GCost;
        public int HCost;
        public int FCost => GCost + HCost;

        public AStarNode Parent;

        /// <summary>
        /// 노드 좌표와 이동 가능 여부를 초기화합니다.
        /// </summary>
        public AStarNode(int x, int y, bool isWalkable)
        {
            X = x;
            Y = y;
            IsWalkable = isWalkable;
        }
    }
}
