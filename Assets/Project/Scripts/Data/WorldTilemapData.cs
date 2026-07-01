using System;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    [Serializable]
    public class WorldTilemapData : ScriptableObject
    {
        [SerializeField] private SerializableDictionary<Vector2Int, WorldTile> mapTiles = new();

        public Dictionary<Vector2Int, WorldTile> MapTiles => mapTiles.ToDictionary();



        /// <summary>
        /// 전체 타일 데이터를 한 번에 교체합니다.
        /// </summary>
        public void SetTiles(IReadOnlyList<WorldTile> tiles)
        {
            Dictionary<Vector2Int, WorldTile> values = new();

            for (int i = 0; i < tiles.Count; i++)
            {
                WorldTile tile = tiles[i];
                values[tile.Pos] = tile;
            }

            mapTiles.SetValues(values);
        }

        /// <summary>
        /// 단일 셀의 맵 데이터를 저장하거나 덮어씁니다.
        /// </summary>
        public void SetTile(WorldTile tile)
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
        public bool TryGetTile(int x, int y, out WorldTile tile)
        {
            return mapTiles.TryGetValue(new Vector2Int(x, y), out tile);
        }
    }
}
