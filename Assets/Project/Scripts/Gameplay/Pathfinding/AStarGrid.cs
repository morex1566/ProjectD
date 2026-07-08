using System;
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

        public Vector3Int Pivot { get; }

        public int NodeCount => Width * Height;

        /// <summary>
        /// WorldTileType 배열을 기반으로 이동 가능한 AStarNode 그리드를 생성합니다.
        /// </summary>
        public AStarGrid(int width, int height, Vector3Int pivot, Func<int, int, bool> isWalkablePredicate)
        {
            Width = width;
            Height = height;
            Pivot = pivot;
            nodes = new AStarNode[width, height];

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    int worldX = x + Pivot.x;
                    int worldY = y + Pivot.y;
                    nodes[x, y] = new AStarNode(worldX, worldY, isWalkablePredicate.Invoke(worldX, worldY));
                }
            }
        }

        /// <summary>
        /// IsInside인지?
        /// Walkable인지?
        /// </summary>
        public bool TryGetNode(int x, int y, out AStarNode node)
        {
            // 외부 좌표는 Tilemap과 같은 월드 셀 좌표입니다.
            node = null;
            if (IsInBound(x, y) == false)
            {
                return false;
            }

            // 이동가능한 노드?
            node = GetNode(x - Pivot.x, y - Pivot.y);
            if (node.IsWalkable == false)
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
            return x >= Pivot.x && x < Pivot.x + Width && y >= Pivot.y && y < Pivot.y + Height;
        }

        public bool IsInBound(Vector3Int cellPos)
        {
            return IsInBound(cellPos.x, cellPos.y);
        }
    }
}
