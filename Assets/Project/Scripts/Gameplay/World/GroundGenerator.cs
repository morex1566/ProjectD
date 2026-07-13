using System;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 지표면 높이를 기준으로 기본 땅 타일을 생성합니다.
    /// </summary>
    [Serializable]
    public sealed class GroundGenerator
    {
        /// <summary>
        /// 지정한 청크 좌표에 기본 지표면과 지층 타일을 생성합니다.
        /// </summary>
        public WorldChunk Generate(Vector2Int chunkCoordinate, float[] surfaceHeights)
        {
            WorldChunk chunk = new WorldChunk(chunkCoordinate);

            int originY = chunkCoordinate.y * WorldChunk.Size;

            for (int localY = 0; localY < WorldChunk.Size; localY++)
            {
                for (int localX = 0; localX < WorldChunk.Size; localX++)
                {
                    int worldY = originY + localY;

                    WorldTileType tileType = SelectTileType(worldY, surfaceHeights[localX]);

                    chunk.SetTile(localX, localY, new WorldTile(tileType));
                }
            }

            return chunk;
        }

        /// <summary>
        /// 월드 좌표와 지표면 높이를 기준으로 기본 타일 종류를 선택합니다.
        /// </summary>
        private WorldTileType SelectTileType(int worldY, float surfaceHeight)
        {
            if (worldY > surfaceHeight)
            {
                return WorldTileType.Empty;
            }

            return WorldTileType.Stone;
        }

        /// <summary>
        /// Inspector에서 들어온 지형 설정값을 유효한 범위로 보정합니다.
        /// </summary>
        public void Validate()
        {
            
        }
    }
}
