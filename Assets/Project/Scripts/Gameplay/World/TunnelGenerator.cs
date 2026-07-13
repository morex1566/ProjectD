using System;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// cave 공간 사이를 이어주는 터널을 생성
    /// </summary>
    [Serializable]
    public sealed class TunnelGenerator
    {
        [SerializeField, Min(0.0001f)] private float frequency = 0.008f;

        [SerializeField, Min(0f)] private float depth = 48f;

        [SerializeField, Min(0f)] private float amplitude = 8f;

        [SerializeField, Min(1f)] private float radius = 4f;


        /// <summary>
        /// 지표면 아래에 노이즈로 높낮이가 변하는 주 통로를 굴착합니다.
        /// </summary>
        public void Generate(WorldChunk chunk, float[] surfaceHeights, int seed)
        {
            FastNoiseLite tunnelNoise = CreateNoise(seed);

            int originX = chunk.Coordinate.x * WorldChunk.Size;
            int originY = chunk.Coordinate.y * WorldChunk.Size;

            for (int localX = 0; localX < WorldChunk.Size; localX++)
            {
                int worldX = originX + localX;

                float tunnelCenterY = GetTunnelCenterY(worldX, surfaceHeights[localX], tunnelNoise);

                for (int localY = 0; localY < WorldChunk.Size; localY++)
                {
                    int worldY = originY + localY;

                    if (IsInsideTunnel(worldY, tunnelCenterY) == false)
                    {
                        continue;
                    }

                    WorldTile tile = chunk.GetTile(localX, localY);
                    if (tile.IsEmpty)
                    {
                        continue;
                    }

                    // 통로에 포함되는 고체 타일을 비웁니다.
                    chunk.SetTile(localX, localY, new WorldTile(WorldTileType.Empty));
                }
            }
        }

        /// <summary>
        /// 해당 X 좌표에서 주 통로의 중심 높이를 계산합니다.
        /// </summary>
        private float GetTunnelCenterY(
            int worldX,
            float surfaceHeight,
            FastNoiseLite tunnelNoise)
        {
            float noiseValue = tunnelNoise.GetNoise(worldX, 0f);

            return surfaceHeight - depth + noiseValue * amplitude;
        }

        /// <summary>
        /// 월드 Y 좌표가 통로 굴착 범위 안인지 확인합니다.
        /// </summary>
        private bool IsInsideTunnel(int worldY, float tunnelCenterY)
        {
            float distanceFromCenter = Mathf.Abs(worldY - tunnelCenterY);

            return distanceFromCenter <= radius;
        }

        /// <summary>
        /// 통로의 높낮이를 결정하는 노이즈를 생성합니다.
        /// </summary>
        private FastNoiseLite CreateNoise(int seed)
        {
            FastNoiseLite noise = new FastNoiseLite(seed);

            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            noise.SetFrequency(frequency);

            return noise;
        }

        /// <summary>
        /// Inspector 설정값을 유효한 범위로 보정합니다.
        /// </summary>
        public void Validate()
        {
            frequency = Mathf.Max(0.0001f, frequency);
            depth = Mathf.Max(0f, depth);
            amplitude = Mathf.Max(0f, amplitude);
            radius = Mathf.Max(1f, radius);
        }
    }
}
