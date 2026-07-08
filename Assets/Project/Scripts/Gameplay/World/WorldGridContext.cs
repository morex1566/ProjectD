using System;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 런타임에서 사용하는 월드 그리드 데이터입니다.
    /// </summary>
    [Serializable]
    public class WorldGridContext
    {
        [SerializeField, ReadOnly] private SerializableDictionary<WorldTilemapType, SerializableDictionary<Vector3Int, WorldTile>> mapTiles = new();


        public SerializableDictionary<WorldTilemapType, SerializableDictionary<Vector3Int, WorldTile>> MapTiles => mapTiles;


        public void SetTile(WorldTilemapType tilemapType, WorldTile tile)
        {
            GetMapTiles(tilemapType).Set(tile.Pos, tile);
        }

        public bool RemoveTile(WorldTilemapType tilemapType, Vector3Int cellPos)
        {
            return GetMapTiles(tilemapType).Remove(cellPos);
        }

        public bool ContainsTile(WorldTilemapType tilemapType, Vector3Int cellPos)
        {
            return GetMapTiles(tilemapType).ContainsKey(cellPos);
        }

        public bool TryGetTile(WorldTilemapType tilemapType, Vector3Int cellPos, out WorldTile tile)
        {
            return GetMapTiles(tilemapType).TryGetValue(cellPos, out tile);
        }

        public void ClearTiles(WorldTilemapType tilemapType)
        {
            GetMapTiles(tilemapType).Clear();
        }

        private SerializableDictionary<Vector3Int, WorldTile> GetMapTiles(WorldTilemapType tilemapType)
        {
            if (mapTiles.TryGetValue(tilemapType, out SerializableDictionary<Vector3Int, WorldTile> tiles) == false || tiles == null)
            {
                tiles = new SerializableDictionary<Vector3Int, WorldTile>();
                tiles.ShowEntriesInInspector = false;
                mapTiles.Set(tilemapType, tiles);
            }

            return tiles;
        }
    }
}
