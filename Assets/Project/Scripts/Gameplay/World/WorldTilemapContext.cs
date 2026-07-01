using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 런타임에서 사용하는 단일 Tilemap 레이어 상태입니다.
    /// </summary>
    [Serializable]
    public class WorldTilemapContext : MonoBehaviour
    {
        [Header(nameof(WorldTilemapContext))]

        [SerializeField, ReadOnly] private WorldGridContext owner;

        [SerializeField] WorldTilemapType tilemapType = WorldTilemapType.WorldTilemapDefault;

        [SerializeField, ReadOnly] private SerializableDictionary<Vector2Int, WorldTile> mapTiles = new();



        public WorldTilemapType TilemapType => tilemapType;



        private void OnValidate()
        {
            // 인스펙터 레이어를 바꾸면
            if (Application.isPlaying)
            {
                SetTilemapType(tilemapType);
            }
            else
            {
                EditorApplication.delayCall += () => SetTilemapType(tilemapType);
            }
        }

        public void SetOwner(WorldGridContext owner)
        {
            this.owner = owner;
        }

        public void SetTilemapType(WorldTilemapType tilemapType)
        {
            this.tilemapType = tilemapType;

            // GameObject.layer에는 LayerMask 비트값이 아니라 Unity Layer 인덱스를 넣어야 합니다.
            int layer = LayerMask.NameToLayer(tilemapType.ToString());
            if (layer >= 0)
            {
                gameObject.layer = layer;
            }

            owner?.Rebuild();
        }

        /// <summary>
        /// 저장용 TilemapData의 현재 값을 런타임 컨텍스트에 반영합니다.
        /// </summary>
        public void SetData(WorldTilemapData tilemapData)
        {
            if (tilemapData == null)
            {
                mapTiles.SetValues(new Dictionary<Vector2Int, WorldTile>());
                return;
            }

            mapTiles.SetValues(tilemapData.MapTiles);
        }

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
        /// 좌표가 해당 Tilemap 레이어에 존재하는지 확인합니다.
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
