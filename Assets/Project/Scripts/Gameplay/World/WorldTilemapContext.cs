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

        [SerializeField, ReadOnly] private WorldGridContext gridContext;

        [SerializeField, ReadOnly] private Dictionary<Vector3Int, WorldTile> mapTiles = new();



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
            RebuildMapTilesFromTilemap();

#if UNITY_EDITOR
            EditorApplication.delayCall -= SetTilemapTypeDelayed;
            EditorApplication.delayCall += SetTilemapTypeDelayed;
#endif
        }

        public void SetGridContext(WorldGridContext owner)
        {
            this.gridContext = owner;
        }

        ///<summary>
        /// Tilemap 타입을 변경하고 타입별 설정을 적용합니다.
        ///</summary>
        public void SetTilemapType(WorldTilemapType tilemapType)
        {
            if (this == null)
            {
                return;
            }

            this.tilemapType = tilemapType;

            ApplyLayerByTilemapType(tilemapType);
            AddComponentsByTilemapType(tilemapType);
            RebuildGridContext();
        }

        ///<summary>
        /// Tilemap 타입 이름과 같은 Unity Layer를 찾아 적용합니다.
        ///</summary>
        private void ApplyLayerByTilemapType(WorldTilemapType tilemapType)
        {
            int layer = LayerMask.NameToLayer(tilemapType.ToString());

            if (layer < 0)
            {
                return;
            }

            gameObject.layer = layer;

            if (tilemapRenderer != null)
            {
                tilemapRenderer.sortingOrder = layer;
            }
        }

        ///<summary>
        /// Tilemap 타입에 따라 필요한 컴포넌트를 구성합니다.
        ///</summary>
        private void AddComponentsByTilemapType(WorldTilemapType tilemapType)
        {
            switch (tilemapType)
            {
                case WorldTilemapType.None:
                    break;

                case WorldTilemapType.WorldTilemapDefault:
                    break;

                case WorldTilemapType.WorldTilemapBackground:
                    break;

                case WorldTilemapType.WorldTilemapGround:

                    if (gameObject.TryGetComponent<Rigidbody2D>(out _) == false)
                    {
                        Rigidbody2D rigid = gameObject.AddComponent<Rigidbody2D>();
                        {
                            rigid.bodyType = RigidbodyType2D.Static;
                            rigid.simulated = true;
                        }
                    }

                    if (gameObject.TryGetComponent<TilemapCollider2D>(out _) == false)
                    {
                        TilemapCollider2D tilemapCollider = gameObject.AddComponent<TilemapCollider2D>();
                        {
                            tilemapCollider.compositeOperation = Collider2D.CompositeOperation.Merge;
                        }
                    }

                    if (gameObject.TryGetComponent<CompositeCollider2D>(out _) == false)
                    {
                        CompositeCollider2D compositeCollider = gameObject.AddComponent<CompositeCollider2D>();
                        {
                            compositeCollider.offsetDistance = 0.005f;
                        }
                    }
                    break;

                case WorldTilemapType.WorldTilemapUI:
                    break;
            }
        }


        ///<summary>
        /// owner가 존재하면 Tilemap 구성을 다시 빌드합니다.
        ///</summary>
        private void RebuildGridContext()
        {
            if (gridContext == null)
            {
                return;
            }

            gridContext.Rebuild();
        }

        /// <summary>
        /// 단일 셀의 맵 데이터를 저장하거나 덮어씁니다.
        /// </summary>
        public void SetTile(WorldTile tile)
        {
            mapTiles[tile.Pos] = tile;
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

        /// <summary>
        /// Unity Tilemap에 배치된 타일을 런타임 지면 판정용 Dictionary로 복원합니다.
        /// </summary>
        private void RebuildMapTilesFromTilemap()
        {
            mapTiles.Clear();

            if (tilemap == null)
            {
                return;
            }

            BoundsInt bounds = tilemap.cellBounds;
            foreach (Vector3Int cellPos in bounds.allPositionsWithin)
            {
                TileBase tileBase = tilemap.GetTile(cellPos);
                if (tileBase == null)
                {
                    continue;
                }

                mapTiles[cellPos] = new WorldTile
                {
                    Pos = cellPos,
                    TileBase = tileBase,
                };
            }
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
