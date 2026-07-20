using System;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 0, 0을 최좌하단으로 하는 월드의 청크를 관리합니다.
    /// </summary>
    public class WorldMap
    {
        private readonly WorldChunk[] chunks;

        private readonly Vector2Int chunkSize;

        private readonly Vector2Int tileSize;

        /// <summary>
        /// 청크 한 변에 포함되는 타일 수입니다.
        /// </summary>
        public int TilesPerChunk { get; }

        public WorldChunk[] Chunks => chunks;

        public int ChunkCount => chunks.Length;

        public Vector2Int ChunkSize => chunkSize;

        public Vector2Int TileSize => tileSize;

        public WorldMap(Vector2Int chunkSize, int tilesPerChunk)
        {
            if (chunkSize.x <= 0 || chunkSize.y <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(chunkSize), $"Chunk size must be positive. Current size: {chunkSize}");
            }

            if (tilesPerChunk <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tilesPerChunk), $"Tiles per chunk must be positive. Current value: {tilesPerChunk}");
            }

            this.chunkSize = chunkSize;
            TilesPerChunk = tilesPerChunk;
            tileSize = chunkSize * tilesPerChunk;
            chunks = new WorldChunk[chunkSize.x * chunkSize.y];
        }

        /// <summary>
        /// 월드에 새로운 청크를 등록합니다.
        /// </summary>
        public void AddChunk(WorldChunk chunk)
        {
            if (chunk == null)
            {
                throw new ArgumentNullException(nameof(chunk));
            }

            if (IsInsideChunkCoordinate(chunk.Coordinate) == false)
            {
                throw new ArgumentOutOfRangeException(nameof(chunk), $"Chunk coordinate is outside the world. Coordinate: {chunk.Coordinate}");
            }

            int chunkIndex = ToChunkIndex(chunk.Coordinate.x, chunk.Coordinate.y);
            if (chunks[chunkIndex] != null)
            {
                throw new InvalidOperationException($"Chunk is already registered. Coordinate: {chunk.Coordinate}");
            }

            chunks[chunkIndex] = chunk;
        }

        /// <summary>
        /// 지정한 청크 좌표에서 청크를 찾습니다.
        /// </summary>
        public bool TryGetChunk(Vector2Int chunkCoordinate, out WorldChunk chunk)
        {
            if (IsInsideChunkCoordinate(chunkCoordinate) == false)
            {
                chunk = null;
                return false;
            }

            chunk = chunks[ToChunkIndex(chunkCoordinate.x, chunkCoordinate.y)];
            return chunk != null;
        }

        /// <summary>
        /// 월드 타일 좌표에 해당하는 타일을 찾습니다.
        /// </summary>
        public bool TryGetTile(Vector2Int worldTileCoordinate, out WorldTile tile)
        {
            if (IsInsideTileCoordinate(worldTileCoordinate) == false)
            {
                tile = default;
                return false;
            }

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
            if (IsInsideTileCoordinate(worldTileCoordinate) == false)
            {
                return false;
            }

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

        /// <summary>
        /// 청크 좌표가 월드 청크 배열 내부인지 확인합니다.
        /// </summary>
        private bool IsInsideChunkCoordinate(Vector2Int coordinate)
        {
            return coordinate.x >= 0 && coordinate.x < chunkSize.x && coordinate.y >= 0 && coordinate.y < chunkSize.y;
        }

        /// <summary>
        /// 타일 좌표가 월드 타일 영역 내부인지 확인합니다.
        /// </summary>
        private bool IsInsideTileCoordinate(Vector2Int coordinate)
        {
            return coordinate.x >= 0 && coordinate.x < tileSize.x && coordinate.y >= 0 && coordinate.y < tileSize.y;
        }

        /// <summary>
        /// 아래에서 위로, 각 행은 왼쪽에서 오른쪽으로 배치되는 청크 배열 인덱스를 반환합니다.
        /// </summary>
        private int ToChunkIndex(int chunkX, int chunkY)
        {
            return chunkX + chunkY * chunkSize.x;
        }
    }
}
