using System;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 월드 청크 생성의 전체 순서를 관리합니다.
    /// </summary>
    [Serializable]
    public class WorldGenerator
    {
        [SerializeField] private int seed = 12345;

        [SerializeField, Min(0.0001f)] private float surfaceFrequency = 0.03f;

        [SerializeField] private float surfaceBaseHeight = 16f;

        [SerializeField, Min(0f)] private float surfaceAmplitude = 8f;

        [SerializeField] private GroundGenerator groundGenerator = new GroundGenerator();

        [SerializeField] private CaveGenerator caveGenerator = new CaveGenerator();

        [SerializeField] private TunnelGenerator tunnelGenerator = new TunnelGenerator();


        public int Seed => seed;


        /// <summary>
        /// 지정한 청크 크기의 월드를 생성합니다.
        /// </summary>
        public WorldMap Generate(Vector2Int chunkSize)
        {
            Validate();

            WorldMap worldMap = new WorldMap();
            FastNoiseLite surfaceNoise = CreateSurfaceNoise();

            for (int chunkX = 0; chunkX < chunkSize.x; chunkX++)
            {
                int originX = chunkX * WorldChunk.Size;
                float[] surfaceHeights = CreateSurfaceHeights(originX, surfaceNoise);

                for (int chunkY = 0; chunkY < chunkSize.y; chunkY++)
                {
                    Vector2Int chunkCoordinate = new Vector2Int(chunkX, chunkY);
                    WorldChunk chunk = groundGenerator.Generate(chunkCoordinate, surfaceHeights);
                    caveGenerator.Generate(chunk, surfaceHeights, seed + 1);

                    worldMap.AddChunk(chunk);
                }
            }

            tunnelGenerator.Generate(worldMap, seed + 2);

            return worldMap;
        }

        /// <summary>
        /// 한 청크 너비에 해당하는 지표면 높이를 계산합니다.
        /// </summary>
        private float[] CreateSurfaceHeights(int originX, FastNoiseLite surfaceNoise)
        {
            float[] surfaceHeights = new float[WorldChunk.Size];

            for (int localX = 0; localX < WorldChunk.Size; localX++)
            {
                int worldX = originX + localX;
                float noiseValue = surfaceNoise.GetNoise(worldX, 0f);

                surfaceHeights[localX] = surfaceBaseHeight + noiseValue * surfaceAmplitude;
            }

            return surfaceHeights;
        }

        /// <summary>
        /// 지표면 높이를 결정하는 노이즈를 생성합니다.
        /// </summary>
        private FastNoiseLite CreateSurfaceNoise()
        {
            FastNoiseLite noise = new FastNoiseLite(seed);
            {
                noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
                noise.SetFrequency(surfaceFrequency);
            }

            return noise;
        }

        /// <summary>
        /// Inspector 설정값을 유효한 범위로 보정합니다.
        /// </summary>
        public void Validate()
        {
            surfaceFrequency = Mathf.Max(0.0001f, surfaceFrequency);
            surfaceAmplitude = Mathf.Max(0f, surfaceAmplitude);

            groundGenerator.Validate();
            caveGenerator.Validate();
            tunnelGenerator.Validate();
        }
    }
}
