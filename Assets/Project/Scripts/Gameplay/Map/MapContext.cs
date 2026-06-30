using System;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 런타임에서 사용하는 맵 상태와 규칙입니다.
    /// </summary>
    [Serializable]
    public class MapContext
    {
        [SerializeField] private Vector3Int pivot = Vector3Int.zero;

        [SerializeField] private Vector3Int startSpawnPoint = Vector3Int.zero;

        [SerializeField, ReadOnly] private SerializableDictionary<Vector2Int, MapTile> MapTiles = new();

        public Vector3Int Pivot => pivot;

        public Vector3Int StartSpawnPoint => startSpawnPoint;



        /// <summary>
        /// 맵 로컬 좌표 기준의 시작 스폰 위치를 저장합니다.
        /// </summary>
        public void SetStartSpawnPoint(Vector3Int cellPos)
        {
            startSpawnPoint = cellPos;
        }

        /// <summary>
        /// 전체 타일 데이터를 한 번에 교체합니다.
        /// </summary>
        public void SetTiles(IReadOnlyList<MapTile> tiles)
        {
            Dictionary<Vector2Int, MapTile> values = new();

            for (int i = 0; i < tiles.Count; i++)
            {
                MapTile tile = tiles[i];
                values[tile.Pos] = tile;
            }

            MapTiles.SetValues(values);
        }

        /// <summary>
        /// 좌표가 맵 범위 안에 있는지 확인합니다.
        /// </summary>
        public bool IsInBounds(int x, int y)
        {
            return MapTiles.ContainsKey(new Vector2Int(x, y));
        }

        /// <summary>
        /// 특정 위치의 타일 데이터를 반환합니다.
        /// </summary>
        public bool TryGetTile(int x, int y, out MapTile tile)
        {
            return MapTiles.TryGetValue(new Vector2Int(x, y), out tile);
        }
    }
}
