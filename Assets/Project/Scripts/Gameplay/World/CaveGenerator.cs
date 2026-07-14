using System;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 생성된 지형에서 노이즈 조건을 만족하는 타일을 비워 동굴을 만듭니다.
    /// </summary>
    [Serializable]
    public class CaveGenerator 
    {
        [SerializeField] private bool isEnabled = true;

        [SerializeField, Min(0.0001f)] private float frequency = 0.035f;

        [SerializeField, Range(-1f, 1f)] private float threshold = -0.1f;

        [SerializeField, Min(0f)] private float surfaceProtectDepth = 8f;

        [SerializeField, Min(0)] private int minimumCaveTileCount = 32;



        /// <summary>
        /// 월드 전체의 고체 타일 일부를 Empty로 변경하고 작은 동굴을 제거합니다.
        /// </summary>
        public void Generate(WorldMap worldMap, float[] surfaceHeights, int seed)
        {
            if (isEnabled == false)
            {
                return;
            }

            FastNoiseLite caveNoise = CreateNoise(seed);

            foreach (WorldChunk chunk in worldMap.Chunks.Values)
            {
                GenerateChunk(chunk, surfaceHeights, caveNoise, worldMap.TilesPerChunk);
            }

            RemoveSmallCaves(worldMap);
        }

        /// <summary>
        /// 지정한 청크에 동굴 타일을 생성합니다.
        /// </summary>
        private void GenerateChunk(WorldChunk chunk, float[] surfaceHeights, FastNoiseLite caveNoise, int tilesPerChunk)
        {
            int originX = chunk.Coordinate.x * tilesPerChunk;
            int originY = chunk.Coordinate.y * tilesPerChunk;

            for (int localY = 0; localY < tilesPerChunk; localY++)
            {
                for (int localX = 0; localX < tilesPerChunk; localX++)
                {
                    WorldTile tile = chunk.GetTile(localX, localY);

                    // 이미 비어 있는 지표면 위쪽은 검사하지 않습니다.
                    if (tile.IsEmpty)
                    {
                        continue;
                    }

                    // 지표면 바로 아래는 동굴이 뚫리지 않도록 보호합니다.
                    int worldX = originX + localX;
                    int worldY = originY + localY;
                    float depth = surfaceHeights[worldX] - worldY;
                    if (depth < this.surfaceProtectDepth)
                    {
                        continue;
                    }

                    // 밀도가 충분히 낮은 영역만 동굴로 비웁니다.
                    float caveDensity = caveNoise.GetNoise(worldX, worldY);
                    if (caveDensity <= threshold)
                    {
                        chunk.SetTile(localX, localY, new WorldTile(WorldTileType.Empty));
                    }
                }
            }
        }

        /// <summary>
        /// 최소 타일 개수보다 작은 동굴 영역을 Stone으로 복구합니다.
        /// </summary>
        private void RemoveSmallCaves(WorldMap worldMap)
        {
            List<List<Vector2Int>> caveRegions = CaveRegionFinder.FindCaveRegions(worldMap);

            foreach (List<Vector2Int> caveRegion in caveRegions)
            {
                if (caveRegion.Count >= minimumCaveTileCount)
                {
                    continue;
                }

                FillCaveWithStone(worldMap, caveRegion);
            }
        }

        /// <summary>
        /// 지정한 동굴 영역의 모든 타일을 Stone으로 변경합니다.
        /// </summary>
        private static void FillCaveWithStone(WorldMap worldMap, List<Vector2Int> caveRegion)
        {
            foreach (Vector2Int coordinate in caveRegion)
            {
                worldMap.TrySetTile(coordinate, new WorldTile(WorldTileType.Stone));
            }
        }

        /// <summary>
        /// 청크 경계가 이어지는 2D 동굴 노이즈를 생성합니다.
        /// </summary>
        private FastNoiseLite CreateNoise(int seed)
        {
            FastNoiseLite noise = new FastNoiseLite(seed);

            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            noise.SetFractalType(FastNoiseLite.FractalType.FBm);
            noise.SetFractalOctaves(3);
            noise.SetFrequency(frequency);

            return noise;
        }
    }
}
