using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// WorldTileType 배열을 A* 탐색용 노드 그래프로 변환해 보관합니다.
    /// </summary>
    public class AStarGrid
    {
        private readonly AStarNode[,] nodes;

        public int Width { get; }

        public int Height { get; }

        public int NodeCount => Width * Height;

        /// <summary>
        /// WorldTileType 배열을 기반으로 이동 가능한 AStarNode 그리드를 생성합니다.
        /// </summary>
        public AStarGrid(int width, int height)
        {
            Width = width;
            Height = height;
            nodes = new AStarNode[width, height];

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    nodes[x, y] = new AStarNode(x, y, true);
                }
            }
        }

        /// <summary>
        /// IsInside인지?
        /// Walkable인지?
        /// </summary>
        public bool TryGetNode(int x, int y, out AStarNode node)
        {
            // 좌표가 그리드 좌표인지?
            node = null;
            if (!IsInBound(x, y))
            {
                return false;
            }

            // 이동가능한 노드?
            node = GetNode(x, y);
            if (!node.IsWalkable)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 좌표에 해당하는 노드를 반환합니다.
        /// </summary>
        public AStarNode GetNode(int x, int y)
        {
            return nodes[x, y];
        }

        /// <summary>
        /// 2차원 배열을 1차원 배열로
        /// </summary>
        public int ToIndex(int x, int y)
        {
            return x + y * Width;
        }

        /// <summary>
        ///  1차원을 다시 2차원으로
        /// </summary>
        public int ToX(int index)
        {
            return index % Width;
        }

        /// <summary>
        ///  1차원을 다시 2차원으로
        /// </summary>
        public int ToY(int index)
        {
            return index / Width;
        }

        /// <summary>
        /// 좌표가 그리드 안 좌표임?
        /// </summary>
        public bool IsInBound(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        public bool IsInBound(Vector3Int cellPos)
        {
            return cellPos.x >= 0 && cellPos.x < Width && cellPos.y >= 0 && cellPos.y < Height;
        }
    }
}
