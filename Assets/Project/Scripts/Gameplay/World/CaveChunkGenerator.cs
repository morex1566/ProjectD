using UnityEngine;

namespace TRPG.Runtime
{
    public sealed class CaveChunkGenerator
    {
        private const float NoiseFrequency = 0.035f;

        private const float SolidThreshold = 0.05f;

        private readonly FastNoiseLite noise;



        public CaveChunkGenerator(int seed)
        {
            noise = new FastNoiseLite(seed);
            noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
            noise.SetFrequency(NoiseFrequency);
        }

        /// <summary>
        /// 지정한 청크 좌표의 동굴 데이터를 생성합니다.
        /// </summary>
        public WorldChunk Generate(Vector2Int chunkCoordinate)
        {
            WorldChunk chunk = new WorldChunk(chunkCoordinate);

            int originX = chunkCoordinate.x * WorldChunk.Size;
            int originY = chunkCoordinate.y * WorldChunk.Size;

            for (int localY = 0; localY < WorldChunk.Size; localY++)
            {
                for (int localX = 0; localX < WorldChunk.Size; localX++)
                {
                    int worldX = originX + localX;
                    int worldY = originY + localY;

                    float density = noise.GetNoise(worldX, worldY);
                    WorldMaterialType materialType = SelectMaterial(density);

                    chunk.SetCell(localX, localY, new WorldCell(materialType));
                }
            }

            return chunk;
        }

        /// <summary>
        /// 노이즈 밀도를 셀의 물질로 변환합니다.
        /// </summary>
        private static WorldMaterialType SelectMaterial(float density)
        {
            if (density >= SolidThreshold)
            {
                return WorldMaterialType.Stone;
            }

            return WorldMaterialType.Empty;
        }
    }
}
