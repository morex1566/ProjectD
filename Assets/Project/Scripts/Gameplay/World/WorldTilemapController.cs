using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TRPG.Runtime
{
    /// <summary>
    /// 단일 Tilemap 레이어의 초기화, 타입 적용, 타일 변경을 처리합니다.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Tilemap))]
    [RequireComponent(typeof(TilemapRenderer))]
    public class WorldTilemapController : MonoBehaviour
    {
        [Header(nameof(WorldTilemapController))]

        [SerializeField, ReadOnly] private WorldGridController gridController;

        [SerializeField, ReadOnly] private TilemapRenderer tilemapRenderer;

        [SerializeField, ReadOnly] private Tilemap tilemap;

        [SerializeField] private WorldTilemapContext context = new();


        public WorldTilemapContext Context => context;

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

        public void Init(bool scheduleEditorApply = true)
        {
            gridController = GetComponentInParent<WorldGridController>();
            tilemapRenderer = GetComponent<TilemapRenderer>();
            tilemap = GetComponent<Tilemap>();

#if UNITY_EDITOR
            if (scheduleEditorApply)
            {
                EditorApplication.delayCall -= SetTilemapTypeDelayed;
                EditorApplication.delayCall += SetTilemapTypeDelayed;
            }
#endif
        }

        public void SetGridController(WorldGridController owner)
        {
            gridController = owner;
        }

        /// <summary>
        /// Tilemap 타입을 변경하고 타입별 설정을 적용합니다.
        /// </summary>
        public void SetTilemapType(WorldTilemapType tilemapType)
        {
            if (this == null)
            {
                return;
            }

            context.TilemapType = tilemapType;
            ApplyLayerByTilemapType(tilemapType);
            AddComponentsByTilemapType(tilemapType);
            gridController.Rebuild();
        }

        /// <summary>
        /// 단일 셀의 맵 데이터를 저장하거나 덮어씁니다.
        /// </summary>
        public void SetTile(WorldTile tile)
        {
            gridController.Context.SetTile(context.TilemapType, tile);
            tilemap.SetTile(tile.Pos, tile.TileBase);
        }

        /// <summary>
        /// 단일 셀의 맵 데이터를 제거합니다.
        /// </summary>
        public void RemoveTile(Vector3Int cellPos)
        {
            gridController.Context.RemoveTile(context.TilemapType, cellPos);
            tilemap.SetTile(cellPos, null);
        }

        /// <summary>
        /// 좌표가 해당 Tilemap 레이어에 존재하는지 확인합니다.
        /// </summary>
        public bool IsInBounds(int x, int y)
        {
            return gridController.Context.ContainsTile(context.TilemapType, new Vector3Int(x, y, 0));
        }

        /// <summary>
        /// 특정 위치의 타일 데이터를 반환합니다.
        /// </summary>
        public bool TryGetTile(int x, int y, out WorldTile tile)
        {
            return gridController.Context.TryGetTile(context.TilemapType, new Vector3Int(x, y, 0), out tile);
        }

        public void Clear()
        {
            gridController.Context.ClearTiles(context.TilemapType);
            tilemap.ClearAllTiles();
        }

        /// <summary>
        /// Tilemap 타입 이름과 같은 Unity Layer를 찾아 적용합니다.
        /// </summary>
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

        /// <summary>
        /// Tilemap 타입에 따라 필요한 컴포넌트를 구성합니다.
        /// </summary>
        private void AddComponentsByTilemapType(WorldTilemapType tilemapType)
        {
            switch (tilemapType)
            {
                case WorldTilemapType.None:
                    break;

                case WorldTilemapType.WorldTilemapDefault:
                    break;

                case WorldTilemapType.WorldTilemapAir:
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

#if UNITY_EDITOR
        private void SetTilemapTypeDelayed()
        {
            if (this == null)
            {
                return;
            }

            SetTilemapType(context.TilemapType);
        }
#endif
    }
}
