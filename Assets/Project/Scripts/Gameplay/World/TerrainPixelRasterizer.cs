using System;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 월드 타일을 렌더링용 청크 픽셀 데이터로 변환합니다.
    /// </summary>
    [Serializable]
    public sealed class TerrainPixelRasterizer
    {
        [SerializeField] private bool isEnabled = true;


        /// <summary>
        /// 월드의 모든 타일을 기본 픽셀 지형으로 변환합니다.
        /// </summary>
        public void Generate(WorldMap worldMap, int pixelsPerTile)
        {
            int pixelsPerChunk = worldMap.TilesPerChunk * pixelsPerTile;

            foreach (WorldChunk chunk in worldMap.Chunks.Values)
            {
                WorldChunkPixelData pixelData = new WorldChunkPixelData(chunk.Coordinate, pixelsPerChunk);

                if (isEnabled == true)
                {
                    RasterizeTiles(chunk, pixelData, worldMap.TilesPerChunk, pixelsPerTile);
                }

                chunk.SetPixelData(pixelData);
            }
        }

        /// <summary>
        /// 청크의 각 타일을 픽셀 영역으로 확대합니다.
        /// </summary>
        private static void RasterizeTiles(WorldChunk chunk, WorldChunkPixelData pixelData, int tilesPerChunk, int pixelsPerTile)
        {
            for (int tileY = 0; tileY < tilesPerChunk; tileY++)
            {
                for (int tileX = 0; tileX < tilesPerChunk; tileX++)
                {
                    WorldTile tile = chunk.GetTile(tileX, tileY);
                    FillTilePixels(pixelData, tileX, tileY, pixelsPerTile, tile.MaterialType);
                }
            }
        }

        /// <summary>
        /// 타일 하나에 해당하는 모든 픽셀을 같은 지형 종류로 채웁니다.
        /// </summary>
        private static void FillTilePixels(WorldChunkPixelData pixelData, int tileX, int tileY, int pixelsPerTile, WorldTileMaterialType type)
        {
            int pixelOriginX = tileX * pixelsPerTile;
            int pixelOriginY = tileY * pixelsPerTile;

            for (int pixelY = 0; pixelY < pixelsPerTile; pixelY++)
            {
                for (int pixelX = 0; pixelX < pixelsPerTile; pixelX++)
                {
                    int localPixelX = pixelOriginX + pixelX;
                    int localPixelY = pixelOriginY + pixelY;

                    pixelData.SetPixel(localPixelX, localPixelY, type);
                }
            }
        }
    }
}
