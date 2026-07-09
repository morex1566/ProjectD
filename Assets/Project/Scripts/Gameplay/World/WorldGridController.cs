using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;


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

        [SerializeField, ReadOnly] private Grid grid = null;

        [SerializeField, ReadOnly] private List<WorldTilemapController> tilemapControllers = new();

        [SerializeField, ReadOnly] private Dictionary<WorldTilemapType, WorldTilemapController> tilemapControllerMap = new();

        [SerializeField] private WorldGridContext context = new();

        [SerializeField] private AStarPathfinder pathfinder = new();


        [Header("Ground")]

        [SerializeField] private WorldTilemapBrush groundBrush = null;

        [SerializeField] private Vector2Int groundSize = Vector2Int.zero;

        [SerializeField] private Vector3Int groundPivot = Vector3Int.zero;


        [Header("Air")]

        [SerializeField] private WorldTilemapBrush airBrush = null;

        [SerializeField] private Vector2Int airSize = Vector2Int.zero;

        [SerializeField] private Vector3Int airPivot = Vector3Int.zero;


        public Grid Grid => grid;

        public WorldGridContext Context => context;


        /// <summary>
        /// 타일맵이 추가되거나 사라지는 경우 캐시를 다시 빌드합니다.
        /// </summary>
        private void OnTransformChildrenChanged()
        {
            Init();
        }

        private void Awake()
        {
            if (Application.isPlaying == false)
            {
                return;
            }

            Init();
        }

        private void Start()
        {
            if (Application.isPlaying == false)
            {
                return;
            }

            // Air 영역을 A* 탐색 범위로 사용하고, Ground 타일이 있는 셀은 이동 불가로 처리합니다.
            pathfinder.Generate(SetWorldWalkablePredicate);
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR
            EditorApplication.delayCall -= Rebuild;
#endif
        }

        private void Init()
        {
            CacheComponents();

            if (Application.isPlaying)
            {
                Rebuild();
            }
            else
            {
#if UNITY_EDITOR
                EditorApplication.delayCall -= Rebuild;
                EditorApplication.delayCall += Rebuild;
#endif
            }
        }

        private void CacheComponents()
        {
            grid = GetComponent<Grid>();
        }

        [ContextMenu(nameof(CreateTilemap))]
        public void CreateTilemap()
        {
            GameObject tilemapInst = new GameObject(nameof(Tilemap));
            tilemapInst.AddComponent<Tilemap>();
            tilemapInst.AddComponent<TilemapRenderer>();
            tilemapInst.transform.SetParent(transform);

            WorldTilemapController tilemapController = tilemapInst.AddComponent<WorldTilemapController>();
            tilemapController.Init(false);
            tilemapController.SetGridController(this);
            tilemapController.SetTilemapType(WorldTilemapType.WorldTilemapDefault);

            tilemapControllers.Add(tilemapController);
        }

        /// <summary>
        /// Tilemap 컨텍스트 조회 캐시를 다시 생성합니다.
        /// </summary>
        public void Rebuild()
        {
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

            // 삭제할 Tile Key 목록을 임시로 저장합니다.
            // 타일맵 데이터 삭제
            List<WorldTilemapType> removeKeys = new();
            foreach (var worldTilemapContext in context.MapTiles)
            {
                // tilemapControllerMap에 없는 Tile이면 삭제 대상으로 등록합니다.
                if (tilemapControllerMap.ContainsKey(worldTilemapContext.Key) == false)
                {
                    removeKeys.Add(worldTilemapContext.Key);
                }
            }
            foreach (WorldTilemapType removeKey in removeKeys)
            {
                // foreach 순회가 끝난 뒤 실제 Dictionary에서 제거합니다.
                context.RemoveTilemap(removeKey);
            }
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

        public bool TryGetRandomTile(WorldTilemapType tilemapType, Predicate<WorldTile> predicate, out WorldTile tile)
        {
            tile = default;

            // 타일 가져오기
            if (context.TryGetMapTiles(tilemapType, out var tiles) == false)
            {
                return false;
            }

            List<WorldTile> candidates = new();
            foreach (WorldTile candidate in tiles.Values)
            {
                if (predicate == null || predicate(candidate))
                {
                    candidates.Add(candidate);
                }
            }

            if (candidates.Count <= 0)
            {
                return false;
            }

            tile = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            return true;
        }

        /// <summary>
        /// 현재 브러시에서 랜덤 타일을 뽑아 지정 좌표에 그립니다.
        /// </summary>
        public void Draw(WorldTilemapBrush brush, WorldTilemapType tilemapType, Vector3Int cellPos)
        {
            if (brush == null)
            {
                return;
            }

            if (brush.TryGetRandomTile(out WorldTile tile) == false)
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

            for (int y = 0; y < groundSize.y; y++)
            {
                for (int x = 0; x < groundSize.x; x++)
                {
                    Draw(groundBrush, WorldTilemapType.WorldTilemapGround, new Vector3Int(x + groundPivot.x, y + groundPivot.y));
                }
            }
        }

        [ContextMenu(nameof(DrawAir))]
        public void DrawAir()
        {
            if (TryGetTilemapController(WorldTilemapType.WorldTilemapAir, out WorldTilemapController tilemapController) == false)
            {
                Debug.LogError($"require {WorldTilemapType.WorldTilemapAir}");
                return;
            }

            tilemapController.Clear();

            for (int y = 0; y < airSize.y; y++)
            {
                for (int x = 0; x < airSize.x; x++)
                {
                    Draw(airBrush, WorldTilemapType.WorldTilemapAir, new Vector3Int(x + airPivot.x, y + airPivot.y));
                }
            }
        }

        private bool SetWorldWalkablePredicate(int x, int y)
        {
            // 그라운드 타일맵 자체가 없다면 걍 다 이동가능한 곳
            if (TryGetTilemapController(WorldTilemapType.WorldTilemapGround, out WorldTilemapController groundTilemap) == false)
            {
                return true;
            }

            // 땅타일이 없으면 이동 가능
            if (groundTilemap.TryGetTile(x, y, out _) == false)
            {
                return true;
            }

            // 땅타일이 있으면 이동 불가능
            return false;
        }
    }
}
