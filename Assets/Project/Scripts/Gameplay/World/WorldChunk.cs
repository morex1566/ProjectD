using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 일정 범위의 월드 타일과 최종 픽셀 지형을 연속 배열로 관리합니다.
    /// </summary>
    public sealed class WorldChunk
    {
        private readonly int tilesPerChunk;

        private readonly int pixelsPerChunk;

        private readonly WorldTile[] tiles;

        private readonly WorldTileMaterialType[] pixels;


        internal WorldTileMaterialType[] Pixels => pixels;

        public int PixelSize => pixelsPerChunk;

        public Vector2Int Coordinate;


        public WorldChunk(Vector2Int coordinate, int tilesPerChunk, int pixelsPerChunk)
        {
            Coordinate = coordinate;
            this.tilesPerChunk = tilesPerChunk;
            this.pixelsPerChunk = pixelsPerChunk;
            tiles = new WorldTile[tilesPerChunk * tilesPerChunk];
            pixels = new WorldTileMaterialType[pixelsPerChunk * pixelsPerChunk];
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
            return tiles[ToTileIndex(localX, localY)];
        }

        /// <summary>
        /// 로컬 좌표의 타일을 교체합니다.
        /// </summary>
        public void SetTile(int localX, int localY, WorldTile tile)
        {
            tiles[ToTileIndex(localX, localY)] = tile;
        }

        /// <summary>
        /// 로컬 픽셀 좌표의 지형 종류를 반환합니다.
        /// </summary>
        public WorldTileMaterialType GetPixel(int localPixelX, int localPixelY)
        {
            return pixels[ToPixelIndex(localPixelX, localPixelY)];
        }

        /// <summary>
        /// 로컬 픽셀 좌표에 지형 종류를 저장합니다.
        /// </summary>
        public void SetPixel(int localPixelX, int localPixelY, WorldTileMaterialType type)
        {
            pixels[ToPixelIndex(localPixelX, localPixelY)] = type;
        }

        /// <summary>
        /// 로컬 픽셀 좌표가 청크 내부인지 확인합니다.
        /// </summary>
        public bool IsInsidePixel(int localPixelX, int localPixelY)
        {
            return localPixelX >= 0 && localPixelX < pixelsPerChunk &&
                   localPixelY >= 0 && localPixelY < pixelsPerChunk;
        }

        /// <summary>
        /// 2차원 로컬 타일 좌표를 연속 배열 인덱스로 변환합니다.
        /// </summary>
        private int ToTileIndex(int localX, int localY)
        {
            return localX + localY * tilesPerChunk;
        }

        /// <summary>
        /// 2차원 로컬 픽셀 좌표를 연속 배열 인덱스로 변환합니다.
        /// </summary>
        private int ToPixelIndex(int localPixelX, int localPixelY)
        {
            return localPixelX + localPixelY * pixelsPerChunk;
        }
    }
}
