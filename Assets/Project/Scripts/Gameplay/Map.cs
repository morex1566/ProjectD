using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    [Serializable]
    public enum MapTileType
    {
        Sea,
        Land
    }

    [Serializable]
    public class MapTile
    {
        [SerializeField] public Vector2 worldPos;

        [SerializeField] public MapTileType type;
    }

    [Serializable]
    public class Chunk
    {
        [SerializeField] public List<MapTile> Tiles;
    }

    /// <summary>
    /// 맵 정보를 저장
    /// </summary>
    [Serializable]
    public class Map
    {
        [SerializeField] public float[] Heights;

        [SerializeField] public MapTile[] Tiles;

        [SerializeField] public Chunk[] Chunks;

        [SerializeField] private int mapLength = 0;

        [SerializeField] private int chunkLength = 0;

        /// <summary>
        /// 한 축에 들어가는 청크 개수입니다.
        /// </summary>
        [SerializeField] private int chunkCountPerAxis = 0;

        public Map(int mapLength, int chunkLength)
        {
            this.mapLength = mapLength;
            this.chunkLength = chunkLength;
            chunkCountPerAxis = mapLength / chunkLength;

            Heights = new float[mapLength * mapLength];
            Tiles = new MapTile[mapLength * mapLength];
            Chunks = new Chunk[chunkCountPerAxis * chunkCountPerAxis];
        }

        /// <summary>
        /// 2차원 타일 좌표를 1차원 배열 인덱스로 변환합니다.
        /// </summary>
        public int ToIndex(int x, int y)
        {
            return x + y * mapLength;
        }

        /// <summary>
        /// 월드 좌표가 포함되는 청크 인덱스를 반환합니다.
        /// </summary>
        public bool TryGetChunkIndex(Vector3 worldPos, out int chunkIndex)
        {
            // 맵 월드 원점이 중앙이므로 월드 좌표를 배열 타일 좌표로 되돌립니다.
            float halfLength = mapLength * 0.5f;
            int tileX = Mathf.FloorToInt(worldPos.x + halfLength);
            int tileY = Mathf.FloorToInt(worldPos.y + halfLength);

            // 타일 좌표를 기준으로 청크 인덱스를 계산합니다.
            return TryGetChunkIndex(tileX, tileY, out chunkIndex);
        }

        public bool TryGetChunkIndex(int tileX, int tileY, out int chunkIndex)
        {
            // 기본값을 실패 상태로 설정합니다.
            chunkIndex = -1;

            // 맵 범위 밖이면 실패합니다.
            if (tileX < 0 || tileX >= mapLength || tileY < 0 || tileY >= mapLength) return false;

            // 타일 좌표가 어느 청크 좌표에 들어가는지 계산합니다.
            // 청크 좌표를 1차원 청크 배열 인덱스로 변환합니다.
            int chunkX = tileX / chunkLength;
            int chunkYFromBottom = tileY / chunkLength;
            int chunkYFromTop = chunkCountPerAxis - 1 - chunkYFromBottom;
            chunkIndex = chunkX + chunkYFromTop * chunkCountPerAxis;

            return true;
        }

        /// <summary>
        /// 월드 좌표가 포함되는 청크를 반환합니다.
        /// </summary>
        public bool TryGetChunk(Vector3 worldPos, out Chunk chunk, out int chunkIndex)
        {
            // 기본값을 실패 상태로 설정합니다.
            chunk = default;

            // 월드 좌표에서 청크 인덱스를 가져옵니다.
            if (TryGetChunkIndex(worldPos, out chunkIndex) == false) return false;

            // 청크 배열에서 해당 청크를 가져옵니다.
            chunk = Chunks[chunkIndex];

            return true;
        }
    }
}
