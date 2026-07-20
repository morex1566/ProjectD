using System;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 지표면 높이를 생성하고 지표면 위의 타일을 비웁니다.
    /// </summary>
    [Serializable]
    public sealed class SurfaceGenerator
    {
        [SerializeField] private bool isEnabled = true;

        [SerializeField, Min(0.0001f)] private float frequency = 0.03f;

        [SerializeField] private float baseHeight = 16f;

        [SerializeField, Min(0f)] private float amplitude = 8f;


        /// <summary>
        /// 월드 전체의 지표면 높이를 생성하고 지표면 위를 Empty로 변경합니다.
        /// </summary>
        public float[] Generate(WorldMap worldMap, int worldWidth, int seed)
        {
            FastNoiseLite surfaceNoise = CreateNoise(seed);
            float[] surfaceHeights = CreateSurfaceHeights(worldWidth, surfaceNoise);

            if (isEnabled == false)
            {
                return surfaceHeights;
            }

            foreach (WorldChunk chunk in worldMap.Chunks.Values)
            {
                CarveChunk(chunk, surfaceHeights, worldMap.TilesPerChunk);
            }

            return surfaceHeights;
        }

        /// <summary>
        /// 월드 전체 너비에 해당하는 지표면 높이를 계산합니다.
        /// </summary>
        private float[] CreateSurfaceHeights(int worldWidth, FastNoiseLite surfaceNoise)
        {
            float[] surfaceHeights = new float[worldWidth];

            for (int worldX = 0; worldX < worldWidth; worldX++)
            {
                float noiseValue = surfaceNoise.GetNoise(worldX, 0f);
                surfaceHeights[worldX] = baseHeight + noiseValue * amplitude;
            }

            return surfaceHeights;
        }

        /// <summary>
        /// 지정한 청크에서 지표면보다 높은 타일을 비웁니다.
        /// </summary>
        private static void CarveChunk(WorldChunk chunk, float[] surfaceHeights, int tilesPerChunk)
        {
            int originX = chunk.Coordinate.x * tilesPerChunk;
            int originY = chunk.Coordinate.y * tilesPerChunk;

            for (int localY = 0; localY < tilesPerChunk; localY++)
            {
                for (int localX = 0; localX < tilesPerChunk; localX++)
                {
                    int worldX = originX + localX;
                    int worldY = originY + localY;

                    if (worldY > surfaceHeights[worldX])
                    {
                        chunk.SetTile(localX, localY, new WorldTile(WorldTileMaterialType.Empty));
                    }
                }
            }
        }

        /// <summary>
        /// 지표면 높이를 결정하는 노이즈를 생성합니다.
        /// </summary>
        private FastNoiseLite CreateNoise(int seed)
        {
            FastNoiseLite noise = new FastNoiseLite(seed);
            {
                noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
                noise.SetFrequency(frequency);
            }

            return noise;
        }
    }
}
