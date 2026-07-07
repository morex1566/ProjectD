using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TRPG.Runtime
{
    /// <summary>
    /// 맵 런타임 객체와 Unity Tilemap 표현을 연결합니다.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Grid))]
    public class WorldGridController : MonoBehaviour
    {
        [Header(nameof(WorldGridController))]

        [SerializeField] private WorldGridContext context = new();

        [SerializeField, ReadOnly] private Grid grid = null;

        [SerializeField, ReadOnly] private List<WorldTilemapController> tilemapControllers = new();

        [SerializeField, ReadOnly] private Dictionary<WorldTilemapType, WorldTilemapController> tilemapControllerMap = new();


        [Header("Ground")]

        [SerializeField] private WorldTilemapBrush groundBrush = null;

        [SerializeField] private Vector2Int size = Vector2Int.zero;

        [SerializeField] private Vector3Int pivot = Vector3Int.zero;


        public WorldGridContext Context => context;

        public Grid Grid => grid;


        /// <summary>
        /// 타일맵이 추가되거나 사라지는 경우 캐시를 다시 빌드합니다.
        /// </summary>
        private void OnTransformChildrenChanged()
        {
            Init();
        }

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
            EditorApplication.delayCall -= RebuildDelayed;
#endif
        }

        private void Init()
        {
            context ??= new WorldGridContext();
            grid = GetComponent<Grid>();

            if (Application.isPlaying)
            {
                Rebuild();
            }
            else
            {
#if UNITY_EDITOR
                EditorApplication.delayCall -= RebuildDelayed;
                EditorApplication.delayCall += RebuildDelayed;
#endif
            }
        }

        [ContextMenu(nameof(CreateTilemap))]
        public void CreateTilemap()
        {
            GameObject tilemapInst = new GameObject(nameof(Tilemap));
            tilemapInst.transform.SetParent(transform);

            tilemapInst.AddComponent<Tilemap>();
            tilemapInst.AddComponent<TilemapRenderer>();

            WorldTilemapController tilemapController = tilemapInst.AddComponent<WorldTilemapController>();

            tilemapController.Init(false);
            tilemapController.SetGridController(this);
            tilemapController.SetTilemapType(WorldTilemapType.WorldTilemapDefault);

            tilemapControllers.Add(tilemapController);

            Rebuild();
        }

        /// <summary>
        /// Tilemap 컨텍스트 조회 캐시를 다시 생성합니다.
        /// </summary>
        public void Rebuild()
        {
            CollectChildTilemapControllers();

            tilemapControllers.RemoveAll(tilemapController => tilemapController == null);
            tilemapControllerMap.Clear();

            foreach (WorldTilemapController tilemapController in tilemapControllers)
            {
                if (tilemapController == null)
                {
                    continue;
                }

                tilemapController.Init(false);
                tilemapController.SetGridController(this);

                if (tilemapController.Context.TilemapType == WorldTilemapType.None)
                {
                    continue;
                }

                tilemapControllerMap[tilemapController.Context.TilemapType] = tilemapController;
            }
        }

        /// <summary>
        /// 지정한 Tilemap 레이어의 런타임 컨텍스트를 반환합니다.
        /// </summary>
        public bool TryGetTilemapContext(WorldTilemapType tilemapType, out WorldTilemapContext tilemapContext)
        {
            tilemapContext = null;

            if (tilemapType == WorldTilemapType.None)
            {
                return false;
            }

            if (tilemapControllerMap.TryGetValue(tilemapType, out WorldTilemapController tilemapController) == false)
            {
                return false;
            }

            tilemapContext = tilemapController.Context;
            return tilemapContext != null;
        }

        /// <summary>
        /// 지정한 Tilemap 레이어의 컨트롤러를 반환합니다.
        /// </summary>
        public bool TryGetTilemapController(WorldTilemapType tilemapType, out WorldTilemapController tilemapController)
        {
            tilemapController = null;

            if (tilemapType == WorldTilemapType.None)
            {
                return false;
            }

            return tilemapControllerMap.TryGetValue(tilemapType, out tilemapController) && tilemapController != null;
        }

        /// <summary>
        /// 지정한 Tilemap 레이어에 좌표 데이터가 있는지 확인합니다.
        /// </summary>
        public bool IsInBounds(WorldTilemapType tilemapType, int x, int y)
        {
            if (TryGetTilemapController(tilemapType, out WorldTilemapController tilemapController) == false)
            {
                return false;
            }

            return tilemapController.IsInBounds(x, y);
        }

        /// <summary>
        /// Ground 레이어를 우선으로 좌표 데이터가 있는지 확인합니다.
        /// </summary>
        public bool IsInBounds(int x, int y)
        {
            return TryGetTile(x, y, out _);
        }

        public bool TryGetTile(WorldTilemapType tilemapType, Vector3 worldPosition, out WorldTile tile)
        {
            tile = default;

            Vector3Int cellPos = grid.WorldToCell(worldPosition);

            if (TryGetTilemapController(tilemapType, out WorldTilemapController tilemapController) == false)
            {
                return false;
            }

            return tilemapController.TryGetTile(cellPos.x, cellPos.y, out tile);
        }

        /// <summary>
        /// 지정한 Tilemap 레이어의 특정 위치 타일 데이터를 반환합니다.
        /// </summary>
        public bool TryGetTile(WorldTilemapType tilemapType, int x, int y, out WorldTile tile)
        {
            tile = default;

            if (TryGetTilemapController(tilemapType, out WorldTilemapController tilemapController) == false)
            {
                return false;
            }

            return tilemapController.TryGetTile(x, y, out tile);
        }

        /// <summary>
        /// Ground 레이어를 우선으로 특정 위치의 타일 데이터를 반환합니다.
        /// </summary>
        public bool TryGetTile(int x, int y, out WorldTile tile)
        {
            if (TryGetTile(WorldTilemapType.WorldTilemapGround, x, y, out tile))
            {
                return true;
            }

            foreach (KeyValuePair<WorldTilemapType, WorldTilemapController> pair in tilemapControllerMap)
            {
                if (pair.Key == WorldTilemapType.WorldTilemapGround)
                {
                    continue;
                }

                if (pair.Value == null)
                {
                    continue;
                }

                WorldTilemapController tilemapController = pair.Value;
                if (tilemapController.TryGetTile(x, y, out tile))
                {
                    return true;
                }
            }

            tile = default;
            return false;
        }

        /// <summary>
        /// 현재 브러시에서 랜덤 타일을 뽑아 지정 좌표에 그립니다.
        /// </summary>
        public void Draw(WorldTilemapType tilemapType, Vector3Int cellPos)
        {
            if (groundBrush == null)
            {
                return;
            }

            if (groundBrush.TryGetRandomTile(out WorldTile tile) == false)
            {
                return;
            }

            if (TryGetTilemapController(tilemapType, out WorldTilemapController tilemapController) == false)
            {
                return;
            }

            tile.Pos = cellPos;
            tilemapController.SetTile(tile);
        }

        [ContextMenu(nameof(DrawGround))]
        public void DrawGround()
        {
            if (TryGetTilemapController(WorldTilemapType.WorldTilemapGround, out WorldTilemapController tilemapController) == false)
            {
                Debug.LogError($"require {WorldTilemapType.WorldTilemapGround}");
                return;
            }

            tilemapController.Clear();

            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    Draw(WorldTilemapType.WorldTilemapGround, new Vector3Int(x + pivot.x, y + pivot.y));
                }
            }
        }

        private void CollectChildTilemapControllers()
        {
            Tilemap[] childTilemaps = GetComponentsInChildren<Tilemap>(true);
            foreach (Tilemap childTilemap in childTilemaps)
            {
                if (childTilemap.TryGetComponent(out WorldTilemapController tilemapController) == false)
                {
                    tilemapController = childTilemap.gameObject.AddComponent<WorldTilemapController>();
                }

                if (tilemapControllers.Contains(tilemapController))
                {
                    continue;
                }

                tilemapControllers.Add(tilemapController);
            }
        }

#if UNITY_EDITOR
        private void RebuildDelayed()
        {
            if (this == null)
            {
                return;
            }

            Rebuild();
        }
#endif
    }
}
