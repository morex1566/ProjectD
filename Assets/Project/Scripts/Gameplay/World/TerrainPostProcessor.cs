using System;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 월드 타일을 렌더링과 충돌 생성에 사용할 픽셀 지형으로 변환합니다.
    /// </summary>
    [Serializable]
    public sealed class TerrainPostProcessor
    {
        private static readonly Vector2Int[] neighborDirections =
        {
            Vector2Int.left,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.up,
        };

        [SerializeField] private bool isEnabled = true;

        [SerializeField, Min(0)] private int edgeRoughnessDepth = 2;

        [SerializeField, Min(0)] private int convexCornerDepth = 2;

        [SerializeField, Min(0.0001f)] private float edgeNoiseFrequency = 0.12f;

        [SerializeField, Range(-1f, 1f)] private float edgeNoiseThreshold = 0.15f;

        [SerializeField, Range(-1f, 1f)] private float convexCornerNoiseThreshold = -0.05f;

        /// <summary>
        /// 생성된 청크 픽셀 지형의 외곽을 후처리합니다.
        /// </summary>
        public void Process(WorldMap worldMap, int seed)
        {
            if (isEnabled == false)
            {
                return;
            }

            FastNoiseLite edgeNoise = CreateEdgeNoise(seed);
            CarveRoughEdges(worldMap, edgeNoise);
        }

        /// <summary>
        /// 노이즈 조건을 만족하는 외곽 픽셀을 지정한 깊이만큼 제거합니다.
        /// </summary>
        private void CarveRoughEdges(WorldMap worldMap, FastNoiseLite edgeNoise)
        {
            int passCount = Mathf.Max(edgeRoughnessDepth, convexCornerDepth);

            for (int pass = 0; pass < passCount; pass++)
            {
                List<(WorldChunk Chunk, Vector2Int Coordinate)> roughPixels = FindRoughBoundaryPixels(worldMap, edgeNoise, pass);

                if (roughPixels.Count == 0)
                {
                    return;
                }

                foreach (var roughPixel in roughPixels)
                {
                    roughPixel.Chunk.PixelData.SetPixel(roughPixel.Coordinate.x, roughPixel.Coordinate.y, WorldTileMaterialType.Empty);
                }
            }
        }

        /// <summary>
        /// 노이즈 조건을 만족하는 현재 외곽 픽셀을 찾습니다.
        /// </summary>
        private List<(WorldChunk Chunk, Vector2Int Coordinate)> FindRoughBoundaryPixels(WorldMap worldMap, FastNoiseLite edgeNoise, int pass)
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
                        bool isConvexCorner = IsConvexCornerPixel(worldMap, chunk, x, y);
                        int maximumDepth = isConvexCorner ? convexCornerDepth : edgeRoughnessDepth;

                        if (pass >= maximumDepth)
                        {
                            continue;
                        }

                        float noiseThreshold = isConvexCorner ? convexCornerNoiseThreshold : edgeNoiseThreshold;

                        if (noiseValue <= noiseThreshold)
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
            if (TryGetPixel(worldMap, chunk, localPixelX, localPixelY, out WorldTileMaterialType type) == false)
            {
                return true;
            }

            return type == WorldTileMaterialType.Empty;
        }

        /// <summary>
        /// 지정한 픽셀이 고체와 빈 공간의 경계인지 확인합니다.
        /// </summary>
        private static bool IsBoundaryPixel(WorldMap worldMap, WorldChunk chunk, int localPixelX, int localPixelY)
        {
            WorldTileMaterialType currentType = chunk.PixelData.GetPixel(localPixelX, localPixelY);

            if (currentType == WorldTileMaterialType.Empty)
            {
                return false;
            }

            foreach (Vector2Int direction in neighborDirections)
            {
                int neighborPixelX = localPixelX + direction.x;
                int neighborPixelY = localPixelY + direction.y;

                if (TryGetPixel(worldMap, chunk, neighborPixelX, neighborPixelY, out WorldTileMaterialType neighborType) == false)
                {
                    return true;
                }

                if (neighborType == WorldTileMaterialType.Empty)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 현재 청크를 기준으로 인접 청크를 포함한 픽셀을 조회합니다.
        /// </summary>
        private static bool TryGetPixel(WorldMap worldMap, WorldChunk sourceChunk, int localPixelX, int localPixelY, out WorldTileMaterialType type)
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
                type = WorldTileMaterialType.Empty;
                return false;
            }

            Vector2Int targetChunkCoordinate = new Vector2Int(
                worldPixelX / pixelsPerChunk,
                worldPixelY / pixelsPerChunk);

            if (worldMap.TryGetChunk(targetChunkCoordinate, out WorldChunk targetChunk) == false)
            {
                type = WorldTileMaterialType.Empty;
                return false;
            }

            if (targetChunk.PixelData == null)
            {
                type = WorldTileMaterialType.Empty;
                return false;
            }

            int targetLocalPixelX = worldPixelX % pixelsPerChunk;
            int targetLocalPixelY = worldPixelY % pixelsPerChunk;

            type = targetChunk.PixelData.GetPixel(targetLocalPixelX, targetLocalPixelY);
            return true;
        }
    }
}
