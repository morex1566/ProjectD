using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 일정 크기의 월드 셀을 연속 배열로 관리합니다.
    /// </summary>
    public sealed class WorldChunk
    {
        public const int Size = 128;

        private readonly WorldCell[] cells = new WorldCell[Size * Size];


        public Vector2Int Coordinate { get; }


        public WorldChunk(Vector2Int coordinate)
        {
            Coordinate = coordinate;
        }

        /// <summary>
        /// 로컬 좌표가 청크 내부인지 확인합니다.
        /// </summary>
        public bool IsInside(int localX, int localY)
        {
            return localX >= 0 &&
                   localX < Size &&
                   localY >= 0 &&
                   localY < Size;
        }

        /// <summary>
        /// 로컬 좌표의 셀을 반환합니다.
        /// </summary>
        public WorldCell GetCell(int localX, int localY)
        {
            return cells[ToIndex(localX, localY)];
        }

        /// <summary>
        /// 로컬 좌표의 셀을 교체합니다.
        /// </summary>
        public void SetCell(int localX, int localY, WorldCell cell)
        {
            cells[ToIndex(localX, localY)] = cell;
        }

        /// <summary>
        /// 2차원 로컬 좌표를 연속 배열 인덱스로 변환합니다.
        /// </summary>
        private static int ToIndex(int localX, int localY)
        {
            return localX + localY * Size;
        }
    }
}
