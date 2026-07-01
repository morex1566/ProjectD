using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TRPG.Runtime
{
    /// <summary>
    /// 런타임에서 사용하는 단일 Tilemap 레이어 상태입니다.
    /// </summary>
    [Serializable]
    public class WorldTilemapContext : MonoBehaviour
    {
        [Header(nameof(WorldTilemapContext))]

        [SerializeField] WorldTilemapType tilemapType = WorldTilemapType.WorldTilemapDefault;

        [SerializeField, ReadOnly] private TilemapRenderer tilemapRenderer;

        [SerializeField, ReadOnly] private Tilemap tilemap;

        [SerializeField, ReadOnly] private WorldGridContext owner;

        [SerializeField, ReadOnly] private SerializableDictionary<Vector3Int, WorldTile> mapTiles = new();



        public WorldTilemapType TilemapType => tilemapType;

        public Tilemap Tilemap => tilemap;



        private void OnValidate()
        {
            Init();
        }

        private void Awake()
        {
            Init();
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR
            EditorApplication.delayCall -= SetTilemapTypeDelayed;
#endif
        }

        public void Init()
        {
            tilemapRenderer = GetComponent<TilemapRenderer>();
            tilemap = GetComponent<Tilemap>();

#if UNITY_EDITOR
            EditorApplication.delayCall -= SetTilemapTypeDelayed;
            EditorApplication.delayCall += SetTilemapTypeDelayed;
#endif
        }

        public void SetOwner(WorldGridContext owner)
        {
            this.owner = owner;
        }

        public void SetTilemapType(WorldTilemapType tilemapType)
        {
            if (this == null)
            {
                return;
            }

            this.tilemapType = tilemapType;
            int layer = LayerMask.NameToLayer(tilemapType.ToString());
            if (layer >= 0)
            {
                gameObject.layer = layer;
            }

            if (owner != null)
            {
                owner.Rebuild();
            }
        }

        /// <summary>
        /// 단일 셀의 맵 데이터를 저장하거나 덮어씁니다.
        /// </summary>
        public void SetTile(WorldTile tile)
        {
            mapTiles.SetValue(tile.Pos, tile);
            tilemap.SetTile(tile.Pos, tile.TileBase);
        }

        /// <summary>
        /// 단일 셀의 맵 데이터를 제거합니다.
        /// </summary>
        public void RemoveTile(Vector3Int cellPos)
        {
            mapTiles.Remove(cellPos);
            tilemap.SetTile(cellPos, null);
        }

        /// <summary>
        /// 좌표가 해당 Tilemap 레이어에 존재하는지 확인합니다.
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

        public void Clear()
        {
            mapTiles.Clear();
            tilemap.ClearAllTiles();
        }

#if UNITY_EDITOR
        private void SetTilemapTypeDelayed()
        {
            if (this == null)
            {
                return;
            }

            SetTilemapType(tilemapType);
        }
#endif
    }
}
