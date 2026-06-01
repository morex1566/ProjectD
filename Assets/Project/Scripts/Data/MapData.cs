using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TRPG.Runtime
{
    [CreateAssetMenu(fileName = "SO_Map", menuName = "Scriptable Objects/Data/Map")]
    public class MapData : ScriptableObject
    {
        [SerializeField] private List<MapTileData> tiles = new();

        [SerializeField] private List<MapMonsterSpawnData> monsterSpawns = new();

        public IReadOnlyList<MapTileData> Tiles => tiles;

        public IReadOnlyList<MapMonsterSpawnData> MonsterSpawns => monsterSpawns;

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

        public void SetMonsterSpawns(IEnumerable<MapMonsterSpawnData> nextMonsterSpawns)
        {
            monsterSpawns.Clear();

            foreach (MapMonsterSpawnData monsterSpawn in nextMonsterSpawns)
            {
                if (!monsterSpawn.HasMonsterDataReference) continue;
                if (!HasTile(monsterSpawn.CellPos)) continue;

                monsterSpawns.Add(monsterSpawn);
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

    [Serializable]
    public class MapMonsterSpawnData
    {
        [SerializeField] private Vector3Int cellPos = Vector3Int.zero;

        [SerializeField] private AssetReferenceT<CreatureData> monsterDataReference = null;

        public Vector3Int CellPos => cellPos;

        public AssetReferenceT<CreatureData> MonsterDataReference => monsterDataReference;

        public bool HasMonsterDataReference => monsterDataReference != null && monsterDataReference.RuntimeKeyIsValid();

        public MapMonsterSpawnData(Vector3Int cellPos, AssetReferenceT<CreatureData> monsterDataReference)
        {
            this.cellPos = cellPos;
            this.monsterDataReference = monsterDataReference;
        }

#if UNITY_EDITOR
        public CreatureData EditorMonsterData => monsterDataReference != null ? monsterDataReference.editorAsset as CreatureData : null;

        public MapMonsterSpawnData(Vector3Int cellPos, CreatureData monsterData)
            : this(cellPos, CreateReference(monsterData))
        {

        }

        private static AssetReferenceT<CreatureData> CreateReference(CreatureData monsterData)
        {
            if (monsterData == null) return null;

            string assetPath = AssetDatabase.GetAssetPath(monsterData);
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            return string.IsNullOrWhiteSpace(guid) ? null : new AssetReferenceT<CreatureData>(guid);
        }
#endif
    }
}
