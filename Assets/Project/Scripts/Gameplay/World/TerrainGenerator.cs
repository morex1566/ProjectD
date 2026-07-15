using System;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 월드 타일을 렌더링과 충돌 생성에 사용할 픽셀 지형으로 변환합니다.
    /// </summary>
    [Serializable]
    public sealed class TerrainPixelGenerator
    {
        private static readonly Vector2Int[] neighborDirections =
        {
            Vector2Int.left,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.up,
        };

        [SerializeField, Min(0)] private int cornerCutDepth = 2;

        [SerializeField, Min(0)] private int edgeRoughnessDepth = 2;

        [SerializeField, Min(0.0001f)] private float edgeNoiseFrequency = 0.12f;

        [SerializeField, Range(-1f, 1f)] private float edgeNoiseThreshold = 0.1f;

        /// <summary>
        /// 월드의 모든 청크에 픽셀 지형 데이터를 생성합니다.
        /// </summary>
        public void Generate(WorldMap worldMap, int pixelsPerTile, int seed)
        {
            int pixelsPerChunk = worldMap.TilesPerChunk * pixelsPerTile;

            foreach (WorldChunk chunk in worldMap.Chunks.Values)
            {
                WorldChunkPixelData pixelData = new WorldChunkPixelData(chunk.Coordinate, pixelsPerChunk);

                RasterizeTiles(chunk, pixelData, worldMap.TilesPerChunk, pixelsPerTile);
                chunk.SetPixelData(pixelData);
            }

            // 타일각을 둥글게
            CarveConvexCorners(worldMap);

            // 모서리를 거칠게
            FastNoiseLite edgeNoise = CreateEdgeNoise(seed);
            CarveRoughEdges(worldMap, edgeNoise);
        }

        /// <summary>
        /// 노이즈 조건을 만족하는 외곽 픽셀을 지정한 깊이만큼 제거합니다.
        /// </summary>
        private void CarveRoughEdges(WorldMap worldMap, FastNoiseLite edgeNoise)
        {
            for (int pass = 0; pass < edgeRoughnessDepth; pass++)
            {
                List<(WorldChunk Chunk, Vector2Int Coordinate)> roughPixels = FindRoughBoundaryPixels(worldMap, edgeNoise);

                if (roughPixels.Count == 0)
                {
                    return;
                }

                foreach (var roughPixel in roughPixels)
                {
                    roughPixel.Chunk.PixelData.SetPixel(roughPixel.Coordinate.x, roughPixel.Coordinate.y, WorldTileType.Empty);
                }
            }
        }

        /// <summary>
        /// 노이즈 조건을 만족하는 현재 외곽 픽셀을 찾습니다.
        /// </summary>
        private List<(WorldChunk Chunk, Vector2Int Coordinate)> FindRoughBoundaryPixels(WorldMap worldMap, FastNoiseLite edgeNoise)
        {
            List<(WorldChunk Chunk, Vector2Int Coordinate)> roughPixels = new();

            foreach (WorldChunk chunk in worldMap.Chunks.Values)
            {
                int pixelSize = chunk.PixelData.Size;

                for (int y = 0; y < pixelSize; y++)
                {
                    for (int x = 0; x < pixelSize; x++)
                    {
                        if (IsBoundaryPixel(worldMap, chunk, x, y) == false)
                        {
                            continue;
                        }

                        int worldPixelX = chunk.Coordinate.x * pixelSize + x;
                        int worldPixelY = chunk.Coordinate.y * pixelSize + y;
                        float noiseValue = edgeNoise.GetNoise(worldPixelX, worldPixelY);
                        if (noiseValue <= edgeNoiseThreshold)
                        {
                            continue;
                        }

                        roughPixels.Add((chunk, new Vector2Int(x, y)));
                    }
                }
            }

            return roughPixels;
        }

        /// <summary>
        /// 청크 경계에서도 연속되는 외곽 노이즈를 생성합니다.
        /// </summary>
        private FastNoiseLite CreateEdgeNoise(int seed)
        {
            FastNoiseLite noise = new FastNoiseLite(seed);
            {
                noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
                noise.SetFrequency(edgeNoiseFrequency);
            }

            return noise;
        }

        /// <summary>
        /// 볼록한 외곽 모서리를 지정한 깊이만큼 단계적으로 깎습니다.
        /// </summary>
        private void CarveConvexCorners(WorldMap worldMap)
        {
            for (int pass = 0; pass < cornerCutDepth; pass++)
            {
                List<(WorldChunk Chunk, Vector2Int Coordinate)> cornerPixels = FindConvexCornerPixels(worldMap);

                if (cornerPixels.Count == 0)
                {
                    return;
                }

                foreach (var cornerPixel in cornerPixels)
                {
                    cornerPixel.Chunk.PixelData.SetPixel(cornerPixel.Coordinate.x, cornerPixel.Coordinate.y, WorldTileType.Empty);
                }
            }
        }

        /// <summary>
        /// 현재 픽셀 상태를 변경하지 않고 모든 볼록 모서리 픽셀을 찾습니다.
        /// </summary>
        private static List<(WorldChunk Chunk, Vector2Int Coordinate)> FindConvexCornerPixels(WorldMap worldMap)
        {
            List<(WorldChunk Chunk, Vector2Int Coordinate)> cornerPixels = new();

            foreach (WorldChunk chunk in worldMap.Chunks.Values)
            {
                int pixelSize = chunk.PixelData.Size;
                for (int y = 0; y < pixelSize; y++)
                {
                    for (int x = 0; x < pixelSize; x++)
                    {
                        if (IsConvexCornerPixel(worldMap, chunk, x, y) == false)
                        {
                            continue;
                        }

                        cornerPixels.Add((chunk, new Vector2Int(x, y)));
                    }
                }
            }

            return cornerPixels;
        }

        /// <summary>
        /// 지정한 외곽 픽셀이 두 빈 공간 사이의 볼록 모서리인지 확인합니다.
        /// </summary>
        private static bool IsConvexCornerPixel(WorldMap worldMap, WorldChunk chunk, int localPixelX, int localPixelY)
        {
            if (IsBoundaryPixel(worldMap, chunk, localPixelX, localPixelY) == false)
            {
                return false;
            }

            bool isLeftEmpty = IsEmptyPixel(worldMap, chunk, localPixelX - 1, localPixelY);
            bool isRightEmpty = IsEmptyPixel(worldMap, chunk, localPixelX + 1, localPixelY);
            bool isBottomEmpty = IsEmptyPixel(worldMap, chunk, localPixelX, localPixelY - 1);
            bool isTopEmpty = IsEmptyPixel(worldMap, chunk, localPixelX, localPixelY + 1);

            return isLeftEmpty && isTopEmpty ||
                   isTopEmpty && isRightEmpty ||
                   isRightEmpty && isBottomEmpty ||
                   isBottomEmpty && isLeftEmpty;
        }

        /// <summary>
        /// 인접 청크와 월드 외부를 포함하여 지정한 픽셀이 빈 공간인지 확인합니다.
        /// </summary>
        private static bool IsEmptyPixel(WorldMap worldMap, WorldChunk chunk, int localPixelX, int localPixelY)
        {
            if (TryGetPixel(worldMap, chunk, localPixelX, localPixelY, out WorldTileType type) == false)
            {
                return true;
            }

            return type == WorldTileType.Empty;
        }

        /// <summary>
        /// 지정한 픽셀이 고체와 빈 공간의 경계인지 확인합니다.
        /// </summary>
        private static bool IsBoundaryPixel(WorldMap worldMap, WorldChunk chunk, int localPixelX, int localPixelY)
        {
            WorldTileType currentType = chunk.PixelData.GetPixel(localPixelX, localPixelY);

            if (currentType == WorldTileType.Empty)
            {
                return false;
            }

            foreach (Vector2Int direction in neighborDirections)
            {
                int neighborPixelX = localPixelX + direction.x;
                int neighborPixelY = localPixelY + direction.y;

                if (TryGetPixel(worldMap, chunk, neighborPixelX, neighborPixelY, out WorldTileType neighborType) == false)
                {
                    return true;
                }

                if (neighborType == WorldTileType.Empty)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 현재 청크를 기준으로 인접 청크를 포함한 픽셀을 조회합니다.
        /// </summary>
        private static bool TryGetPixel(WorldMap worldMap, WorldChunk sourceChunk, int localPixelX, int localPixelY, out WorldTileType type)
        {
            WorldChunkPixelData sourcePixelData = sourceChunk.PixelData;

            if (sourcePixelData.IsInside(localPixelX, localPixelY))
            {
                type = sourcePixelData.GetPixel(localPixelX, localPixelY);
                return true;
            }

            int pixelsPerChunk = sourcePixelData.Size;
            int worldPixelX = sourceChunk.Coordinate.x * pixelsPerChunk + localPixelX;
            int worldPixelY = sourceChunk.Coordinate.y * pixelsPerChunk + localPixelY;

            if (worldPixelX < 0 || worldPixelY < 0)
            {
                type = WorldTileType.Empty;
                return false;
            }

            Vector2Int targetChunkCoordinate = new Vector2Int(
                worldPixelX / pixelsPerChunk,
                worldPixelY / pixelsPerChunk);

            if (worldMap.TryGetChunk(targetChunkCoordinate, out WorldChunk targetChunk) == false)
            {
                type = WorldTileType.Empty;
                return false;
            }

            if (targetChunk.PixelData == null)
            {
                type = WorldTileType.Empty;
                return false;
            }

            int targetLocalPixelX = worldPixelX % pixelsPerChunk;
            int targetLocalPixelY = worldPixelY % pixelsPerChunk;

            type = targetChunk.PixelData.GetPixel(targetLocalPixelX, targetLocalPixelY);
            return true;
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
                    FillTilePixels(pixelData, tileX, tileY, pixelsPerTile, tile.Type);
                }
            }
        }

        /// <summary>
        /// 타일 하나에 해당하는 모든 픽셀을 같은 지형 종류로 채웁니다.
        /// </summary>
        private static void FillTilePixels(WorldChunkPixelData pixelData, int tileX, int tileY, int pixelsPerTile, WorldTileType type)
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
