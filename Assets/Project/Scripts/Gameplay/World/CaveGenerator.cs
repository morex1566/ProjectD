using System;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 생성된 지형에서 노이즈 조건을 만족하는 타일을 비워 동굴을 만듭니다.
    /// </summary>
    [Serializable]
    public class CaveGenerator 
    {
        [SerializeField, Min(0.0001f)] private float frequency = 0.035f;

        [SerializeField, Range(-1f, 1f)] private float threshold = -0.1f;

        [SerializeField, Min(0f)] private float depth = 8f;



        /// <summary>
        /// 지정한 청크의 고체 타일 일부를 Empty로 변경합니다.
        /// </summary>
        public void Generate(WorldChunk chunk, float[] surfaceHeights, int seed)
        {
            FastNoiseLite caveNoise = CreateNoise(seed);

            int originX = chunk.Coordinate.x * WorldChunk.Size;
            int originY = chunk.Coordinate.y * WorldChunk.Size;

            for (int localY = 0; localY < WorldChunk.Size; localY++)
            {
                for (int localX = 0; localX < WorldChunk.Size; localX++)
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
                    float depth = surfaceHeights[localX] - worldY;
                    if (depth < this.depth)
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

        /// <summary>
        /// Inspector 설정값을 유효한 범위로 보정합니다.
        /// </summary>
        public void Validate()
        {
            frequency = Mathf.Max(0.0001f, frequency);
            threshold = Mathf.Clamp(threshold, -1f, 1f);
            depth = Mathf.Max(0f, depth);
        }
    }
}
