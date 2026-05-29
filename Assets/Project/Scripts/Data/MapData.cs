using System;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    [CreateAssetMenu(fileName = "SO_Map", menuName = "Scriptable Objects/Data/Map")]
    public class MapData : ScriptableObject
    {
        [SerializeField] private List<MapTileData> tiles = new();

        public IReadOnlyList<MapTileData> Tiles => tiles;

        public bool TryGetTilePb(Vector3Int cellPos, out TileController tilePb)
        {
            foreach (MapTileData tile in tiles)
            {
                if (tile.CellPos != cellPos) continue;

                tilePb = tile.TilePb;
                return tilePb != null;
            }

            tilePb = null;
            return false;
        }

        public bool HasTile(Vector3Int cellPos)
        {
            return TryGetTilePb(cellPos, out _);
        }

        public void SetTiles(IEnumerable<MapTileData> nextTiles)
        {
            tiles.Clear();

            foreach (MapTileData tile in nextTiles)
            {
                if (tile.TilePb == null) continue;

                tiles.Add(tile);
            }
        }

        public BoundsInt GetBounds()
        {
            if (tiles.Count == 0)
            {
                return new BoundsInt(Vector3Int.zero, Vector3Int.zero);
            }

            Vector3Int min = tiles[0].CellPos;
            Vector3Int max = tiles[0].CellPos;
            foreach (MapTileData tile in tiles)
            {
                min = Vector3Int.Min(min, tile.CellPos);
                max = Vector3Int.Max(max, tile.CellPos);
            }

            return new BoundsInt(min, max - min + Vector3Int.one);
        }
    }

    [Serializable]
    public class MapTileData
    {
        [SerializeField] private Vector3Int cellPos = Vector3Int.zero;

        [SerializeField] private TileController tilePb = null;

        public Vector3Int CellPos => cellPos;

        public TileController TilePb => tilePb;

        public MapTileData(Vector3Int cellPos, TileController tilePb)
        {
            this.cellPos = cellPos;
            this.tilePb = tilePb;
        }
    }
}
