using System;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    [Serializable]
    public class WorldTilemapData : ScriptableObject
    {
        [SerializeField] private SerializableDictionary<Vector3Int, WorldTile> mapTiles = new();

        public IReadOnlyDictionary<Vector3Int, WorldTile> MapTiles => mapTiles.ReadOnlyDictionary;



        /// <summary>
        /// 단일 셀의 맵 데이터를 저장하거나 덮어씁니다.
        /// </summary>
        public void SetTile(WorldTile tile)
        {
            mapTiles.Set(tile.Pos, tile);
        }

        /// <summary>
        /// 단일 셀의 맵 데이터를 제거합니다.
        /// </summary>
        public bool RemoveTile(Vector3Int cellPos)
        {
            return mapTiles.Remove(cellPos);
        }

        /// <summary>
        /// 좌표가 맵 범위 안에 있는지 확인합니다.
        /// </summary>
        public bool IsInBounds(int x, int y)
        {
            return mapTiles.ContainsKey(new Vector3Int(x, y, 0));
        }

        /// <summary>
        /// 특정 위치의 타일 데이터를 반환합니다.
        /// </summary>
        public bool TryGetTile(int x, int y, out WorldTile tile)
        {
            return mapTiles.TryGetValue(new Vector3Int(x, y, 0), out tile);
        }
    }
}
