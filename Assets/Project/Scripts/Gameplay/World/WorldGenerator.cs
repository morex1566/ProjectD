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

        [SerializeField] private TerrainPixelRasterizer terrainPixelRasterizer = new TerrainPixelRasterizer();

        [SerializeField] private TerrainPostProcessor terrainPostProcessor = new TerrainPostProcessor();

        private float[] surfaceHeights = null;


        public int Seed => seed;


        /// <summary>
        /// 지정한 청크 크기의 월드를 생성합니다.
        /// </summary>
        public WorldMap Generate(WorldGenerationSettingsData settings)
        {
            WorldMap worldMap = CreateChunks(settings.ChunkSize, settings.TilesPerChunk, settings.PixelsPerTile);
            int worldWidth = settings.ChunkSize.x * settings.TilesPerChunk;

            // CAUTION : 순서 중요합니다...
            groundGenerator.Generate(worldMap);
            surfaceHeights = surfaceGenerator.Generate(worldMap, worldWidth, seed);
            caveGenerator.Generate(worldMap, surfaceHeights, seed + 1);
            tunnelGenerator.Generate(worldMap, seed + 2);
            terrainPixelRasterizer.Generate(worldMap, settings.PixelsPerTile);
            terrainPostProcessor.Process(worldMap, seed + 3);

            return worldMap;
        }

        /// <summary>
        /// 지정한 크기만큼 빈 월드 청크를 생성합니다.
        /// </summary>
        private static WorldMap CreateChunks(Vector2Int chunkSize, int tilesPerChunk, int pixelsPerTile)
        {
            WorldMap worldMap = new WorldMap(chunkSize, tilesPerChunk);
            int pixelsPerChunk = tilesPerChunk * pixelsPerTile;

            // 청크 배열을 아래에서 위로, 각 행은 왼쪽에서 오른쪽으로 채웁니다.
            for (int chunkY = 0; chunkY < chunkSize.y; chunkY++)
            {
                for (int chunkX = 0; chunkX < chunkSize.x; chunkX++)
                {
                    Vector2Int chunkCoordinate = new Vector2Int(chunkX, chunkY);
                    worldMap.AddChunk(new WorldChunk(chunkCoordinate, tilesPerChunk, pixelsPerChunk));
                }
            }

            return worldMap;
        }
    }
}
