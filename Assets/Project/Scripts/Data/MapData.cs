using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TRPG.Runtime
{
    /// <summary>
    /// 저장 가능한 맵 원본 데이터입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_Map", menuName = "Scriptable Objects/MapController")]
    public class MapData : ScriptableObject
    {
        [SerializeField] private Vector3Int pivot = Vector3Int.zero;

        [SerializeField] private Vector3Int startSpawnPoint = Vector3Int.zero;

        private SerializableDictionary<Vector2Int, Tile> mapTiles = new();

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
        public void SetTiles(IReadOnlyList<Tile> tiles)
        {
            Dictionary<Vector2Int, Tile> values = new();

            for (int i = 0; i < tiles.Count; i++)
            {
                Tile tile = tiles[i];
                values[tile.Pos] = tile;
            }

            mapTiles.SetValues(values);
        }

        /// <summary>
        /// 단일 셀의 맵 데이터를 저장하거나 덮어씁니다.
        /// </summary>
        public void SetTile(Tile tile)
        {
            mapTiles.SetValue(tile.Pos, tile);
        }

        /// <summary>
        /// 단일 셀의 맵 데이터를 제거합니다.
        /// </summary>
        public bool RemoveTile(Vector2Int cellPos)
        {
            return mapTiles.Remove(cellPos);
        }

        /// <summary>
        /// 좌표가 맵 범위 안에 있는지 확인합니다.
        /// </summary>
        public bool IsInBounds(int x, int y)
        {
            return mapTiles.ContainsKey(new Vector2Int(x, y));
        }

        /// <summary>
        /// 특정 위치의 타일 데이터를 반환합니다.
        /// </summary>
        public bool TryGetTile(int x, int y, out Tile tile)
        {
            return mapTiles.TryGetValue(new Vector2Int(x, y), out tile);
        }
    }
}
