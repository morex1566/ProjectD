using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 0, 0을 최좌하단으로 하는 월드의 청크를 관리합니다.
    /// </summary>
    public class WorldMap
    {
        private readonly Dictionary<Vector2Int, WorldChunk> chunks = new();

        /// <summary>
        /// 청크 한 변에 포함되는 타일 수입니다.
        /// </summary>
        public int TilesPerChunk { get; }

        public IReadOnlyDictionary<Vector2Int, WorldChunk> Chunks => chunks;

        public int ChunkCount => chunks.Count;

        public WorldMap(int tilesPerChunk)
        {
            TilesPerChunk = tilesPerChunk;
        }

        /// <summary>
        /// 월드에 새로운 청크를 등록합니다.
        /// </summary>
        public void AddChunk(WorldChunk chunk)
        {
            chunks.Add(chunk.Coordinate, chunk);
        }

        /// <summary>
        /// 지정한 청크 좌표에서 청크를 찾습니다.
        /// </summary>
        public bool TryGetChunk(Vector2Int chunkCoordinate, out WorldChunk chunk)
        {
            return chunks.TryGetValue(chunkCoordinate, out chunk);
        }

        /// <summary>
        /// 월드 타일 좌표에 해당하는 타일을 찾습니다.
        /// </summary>
        public bool TryGetTile(Vector2Int worldTileCoordinate, out WorldTile tile)
        {
            Vector2Int chunkCoordinate = WorldToChunkCoordinate(worldTileCoordinate);

            if (TryGetChunk(chunkCoordinate, out WorldChunk chunk) == false)
            {
                tile = default;
                return false;
            }

            Vector2Int localCoordinate = WorldToLocalCoordinate(worldTileCoordinate);

            tile = chunk.GetTile(localCoordinate.x, localCoordinate.y);

            return true;
        }

        /// <summary>
        /// 월드 타일 좌표에 새로운 타일을 저장합니다.
        /// </summary>
        public bool TrySetTile(Vector2Int worldTileCoordinate, WorldTile tile)
        {
            Vector2Int chunkCoordinate = WorldToChunkCoordinate(worldTileCoordinate);

            if (TryGetChunk(chunkCoordinate, out WorldChunk chunk) == false)
            {
                return false;
            }

            Vector2Int localCoordinate = WorldToLocalCoordinate(worldTileCoordinate);

            chunk.SetTile(localCoordinate.x, localCoordinate.y, tile);

            return true;
        }

        /// <summary>
        /// 월드 타일 좌표를 청크 좌표로 변환합니다.
        /// </summary>
        public Vector2Int WorldToChunkCoordinate(Vector2Int worldTileCoordinate)
        {
            return new Vector2Int(worldTileCoordinate.x / TilesPerChunk, worldTileCoordinate.y / TilesPerChunk);
        }

        /// <summary>
        /// 월드 타일 좌표를 청크 내부의 로컬 좌표로 변환합니다.
        /// </summary>
        public Vector2Int WorldToLocalCoordinate(Vector2Int worldTileCoordinate)
        {
            return new Vector2Int(worldTileCoordinate.x % TilesPerChunk, worldTileCoordinate.y % TilesPerChunk);
        }
    }
}
