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

        [SerializeField] private GroundGenerator groundGenerator = new GroundGenerator();

        [SerializeField] private SurfaceGenerator surfaceGenerator = new SurfaceGenerator();

        [SerializeField] private CaveGenerator caveGenerator = new CaveGenerator();

        [SerializeField] private TunnelGenerator tunnelGenerator = new TunnelGenerator();

        private float[] surfaceHeights = null;


        public int Seed => seed;


        /// <summary>
        /// 지정한 청크 크기의 월드를 생성합니다.
        /// </summary>
        public WorldMap Generate(WorldGenerationSettingsData setting)
        {
            WorldMap worldMap = CreateChunks(setting.ChunkSize, setting.TilesPerChunk);
            int worldWidth = setting.ChunkSize.x * setting.TilesPerChunk;

            // CAUTION : 순서 중요합니다...
            groundGenerator.Generate(worldMap);
            surfaceHeights = surfaceGenerator.Generate(worldMap, worldWidth, seed);
            caveGenerator.Generate(worldMap, surfaceHeights, seed + 1);
            tunnelGenerator.Generate(worldMap, seed + 2);

            return worldMap;
        }

        /// <summary>
        /// 지정한 크기만큼 빈 월드 청크를 생성합니다.
        /// </summary>
        private static WorldMap CreateChunks(Vector2Int chunkSize, int tilesPerChunk)
        {
            WorldMap worldMap = new WorldMap(tilesPerChunk);

            for (int chunkX = 0; chunkX < chunkSize.x; chunkX++)
            {
                for (int chunkY = 0; chunkY < chunkSize.y; chunkY++)
                {
                    Vector2Int chunkCoordinate = new Vector2Int(chunkX, chunkY);
                    worldMap.AddChunk(new WorldChunk(chunkCoordinate, tilesPerChunk));
                }
            }

            return worldMap;
        }
    }
}
