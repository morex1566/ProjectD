using System;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 월드의 모든 타일을 기본 지층으로 채웁니다.
    /// </summary>
    [Serializable]
    public sealed class GroundGenerator
    {
        [SerializeField] private bool isEnabled = true;


        /// <summary>
        /// 월드 전체를 Stone 타일로 채웁니다.
        /// </summary>
        public void Generate(WorldMap worldMap)
        {
            if (isEnabled == false)
            {
                return;
            }

            foreach (WorldChunk chunk in worldMap.Chunks)
            {
                for (int localY = 0; localY < worldMap.TilesPerChunk; localY++)
                {
                    for (int localX = 0; localX < worldMap.TilesPerChunk; localX++)
                    {
                        chunk.SetTile(localX, localY, new WorldTile(WorldTileMaterialType.Stone));
                    }
                }
            }
        }
    }
}
