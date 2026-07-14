using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 일정 범위의 월드 타일을 연속 배열로 관리합니다.
    /// </summary>
    public sealed class WorldChunk
    {
        private readonly int tilesPerChunk;

        private readonly WorldTile[] tiles;


        public Vector2Int Coordinate { get; }


        public WorldChunk(Vector2Int coordinate, int tilesPerChunk)
        {
            Coordinate = coordinate;
            this.tilesPerChunk = tilesPerChunk;
            tiles = new WorldTile[tilesPerChunk * tilesPerChunk];
        }

        /// <summary>
        /// 로컬 타일 좌표가 청크 내부인지 확인합니다.
        /// </summary>
        public bool IsInside(int localX, int localY)
        {
            return localX >= 0 && localX < tilesPerChunk && localY >= 0 && localY < tilesPerChunk;
        }

        /// <summary>
        /// 로컬 좌표의 타일을 반환합니다.
        /// </summary>
        public WorldTile GetTile(int localX, int localY)
        {
            return tiles[ToIndex(localX, localY)];
        }

        /// <summary>
        /// 로컬 좌표의 타일을 교체합니다.
        /// </summary>
        public void SetTile(int localX, int localY, WorldTile tile)
        {
            tiles[ToIndex(localX, localY)] = tile;
        }

        /// <summary>
        /// 2차원 로컬 좌표를 연속 배열 인덱스로 변환합니다.
        /// </summary>
        private int ToIndex(int localX, int localY)
        {
            return localX + localY * tilesPerChunk;
        }
    }
}
