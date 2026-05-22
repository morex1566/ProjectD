using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TRPG.Runtime
{
    public class WorldManager : MonoBehaviourSingleton<WorldManager>
    {
        [Header("Runtime")]

        [SerializeField, ReadOnly] private List<Tilemap> tilemaps = null;

        /// <summary>
        /// Creature가 이동할 수 있는 Tilemap
        /// </summary>
        [SerializeField, ReadOnly] private List<Tilemap> ground = null;

        [SerializeField, ReadOnly] private Dictionary<int, CreatureController> creatures = null;

        [Header("Setup")]

        [SerializeField] private GameObject monsterPb = null;
        [SerializeField] private GameObject playerPb = null;
        [SerializeField] private GameObject allyMovableTilePb = null;
        [SerializeField] private GameObject enemyMovableTilePb = null;

        private const float IndicatorCloseDestroyDelay = 0.35f;

        private readonly Dictionary<Vector3Int, GameObject> movableIndicators = new();

        private CreatureController shownMoveRangeOwner = null;

        private void OnValidate()
        {
            Init();
        }

        private void Awake()
        {
            Init();

            ResourceManager.Database.Load();

            // 인스턴싱
            SpawnMonster(ResourceManager.Database.GetMonsterData("Monster_00"), new Vector3Int(0, 2, 0));
            SpawnPlayer(new Vector3Int(0, 0, 0));
        }

        private void Init()
        {
            tilemaps = new List<Tilemap>();
            ground = new List<Tilemap>();
            creatures = new Dictionary<int, CreatureController>();

            // 모든 타일맵을 매핑
            Grid[] grids = FindObjectsByType<Grid>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID);
            foreach (Grid grid in grids)
            {
                tilemaps.AddRange(grid.GetComponentsInChildren<Tilemap>());
            }

            // 모든 Ground(이동가능 타일맵)을 매핑
            foreach (Tilemap tilemap in tilemaps)
            {
                if (tilemap.gameObject.layer != UnityConstant.Layers.GroundIndex) continue;

                ground.Add(tilemap);
            }
        }

        /// <summary>
        /// 월드 좌표 아래에 Ground 타일이 있으면 해당 셀 좌표를 반환합니다.
        /// </summary>
        public bool TryGetGroundCellPosition(Vector3 worldPos, out Vector3Int cellPos)
        {
            if (ground == null)
            {
                cellPos = default;
                return false;
            }

            foreach (Tilemap tilemap in ground)
            {
                Vector3Int candidateCellPosition = tilemap.WorldToCell(worldPos);
                if (!tilemap.HasTile(candidateCellPosition)) continue;

                cellPos = candidateCellPosition;

                return true;
            }

            cellPos = default;

            return false;
        }

        /// <summary>
        /// Ground 셀 좌표가 유효하면 해당 셀의 월드 중심 좌표를 반환합니다.
        /// </summary>
        public bool TryGetGroundWorldPosition(Vector3Int cellPos, out Vector3 worldPos)
        {
            if (ground == null) Init();

            foreach (Tilemap tilemap in ground)
            {
                if (!tilemap.HasTile(cellPos)) continue;

                worldPos = tilemap.GetCellCenterWorld(cellPos);

                return true;
            }

            worldPos = default;

            return false;
        }

        public void SpawnMonster(CreatureData monsterData, Vector3Int cellPos)
        {
            // 인스턴싱할 수 없는 위치
            if (!TryGetGroundWorldPosition(cellPos, out Vector3 worldPos)) return;

            // 몬스터 생성
            GameObject monsterInst = Instantiate(monsterPb, worldPos, Quaternion.identity);
            MonsterController monsterController = monsterInst.GetComponent<MonsterController>();
            if (monsterController == null)
            {
                Debug.LogWarning($"SpawnMonster failed. MonsterController not found. Prefab: {monsterPb.name}");
                Destroy(monsterInst);
                return;
            }
            monsterController.Model.Init(cellPos, monsterData);

            // 몬스터 등록
            creatures.Add(monsterController.GetInstanceID(), monsterController);
        }

        public void SpawnPlayer(Vector3Int cellPos)
        {
            // 인스턴싱할 수 없는 위치
            if (!TryGetGroundWorldPosition(cellPos, out Vector3 worldPos)) return;

            // 플레이어 생성
            GameObject playerInst = Instantiate(playerPb, worldPos, Quaternion.identity);
            PlayerController playerController = playerInst.GetComponent<PlayerController>();
            if (playerController == null)
            {
                Debug.LogWarning($"SpawnPlayer failed. PlayerController not found. Prefab: {playerPb.name}");
                Destroy(playerInst);
                return;
            }
            playerController.Model.Init(cellPos);

            // 플레이어 등록
            creatures.Add(playerController.GetInstanceID(), playerController);
        }

        public void Despawn(int instanceId)
        {
            Destroy(creatures[instanceId].gameObject);
            creatures.Remove(instanceId);
        }

        public GameObject AllyMovableTilePb => allyMovableTilePb;

        public GameObject EnemyMovableTilePb => enemyMovableTilePb;

        /// <summary>
        /// 크리처의 이동 범위 안에 있는 모든 Ground 타일을 지정된 프리팹으로 표시합니다.
        /// </summary>
        public void ShowMoveRange(CreatureController owner, int moveRange, GameObject indicatorPrefab)
        {
            ClearMoveRange();

            if (owner == null) return;
            if (moveRange <= 0) return;

            shownMoveRangeOwner = owner;
            Vector3Int originCellPos = owner.Model.CellPos;
            for (int x = -moveRange; x <= moveRange; x++)
            {
                for (int y = -moveRange; y <= moveRange; y++)
                {
                    int distance = Mathf.Abs(x) + Mathf.Abs(y);
                    if (distance == 0 || distance > moveRange) continue;

                    Vector3Int cellPos = originCellPos + new Vector3Int(x, y, 0);
                    if (!TryGetGroundWorldPosition(cellPos, out Vector3 worldPos)) continue;

                    AddIndicator(movableIndicators, indicatorPrefab, cellPos, worldPos);
                }
            }
        }

        /// <summary>
        /// 현재 표시 중인 이동 가능 타일 표시를 모두 제거합니다.
        /// </summary>
        public void ClearMoveRange()
        {
            ClearIndicators(movableIndicators);
            shownMoveRangeOwner = null;
        }

        public bool IsMovableHighlighted(Vector3Int cellPos)
        {
            return movableIndicators.ContainsKey(cellPos);
        }

        public bool IsMoveRangeOwner(CreatureController owner)
        {
            return shownMoveRangeOwner == owner;
        }

        /// <summary>
        /// 이 위치에 몬스터가 있는지 확인합니다. 
        /// </summary>
        public bool HasMonster(Vector3Int cellPos, out MonsterController monsterController)
        {
            foreach (KeyValuePair<int, CreatureController> pair in creatures)
            {
                // 위치에 creature가 없음
                if (cellPos != pair.Value.Model.CellPos) continue;

                // creature가 monster가 아님
                if (pair.Value is not MonsterController castedController) continue;

                monsterController = castedController;
                return true;
            }

            monsterController = null;
            return false;
        }

        public bool HasMonsterAtWorld(Vector3 worldPos, out MonsterController monsterController)
        {
            if (!TryGetGroundCellPosition(worldPos, out Vector3Int cellPos))
            {
                monsterController = null;
                return false;
            }

            // 타일 기반 클릭 판정은 스프라이트 bounds가 아니라 점유 셀을 기준으로 합니다.
            return HasMonster(cellPos, out monsterController);
        }

        private void AddIndicator(Dictionary<Vector3Int, GameObject> indicators, GameObject prefab, Vector3Int cellPos, Vector3 worldPos)
        {
            if (prefab == null)
            {
                Debug.LogWarning("Move range indicator prefab is not assigned.");
                return;
            }

            GameObject indicator = Instantiate(prefab, worldPos, Quaternion.identity, transform);
            indicators.Add(cellPos, indicator);
            PlayIndicatorTrigger(indicator, UnityConstant.Animator.Parameters.AC_TIleIndicator.Trigger.OnOpen);
        }

        private void ClearIndicators(Dictionary<Vector3Int, GameObject> indicators)
        {
            foreach (KeyValuePair<Vector3Int, GameObject> pair in indicators)
            {
                if (pair.Value == null) continue;

                CloseIndicator(pair.Value);
            }

            indicators.Clear();
        }

        private void CloseIndicator(GameObject indicator)
        {
            PlayIndicatorTrigger(indicator, UnityConstant.Animator.Parameters.AC_TIleIndicator.Trigger.OnClose);
            Destroy(indicator, IndicatorCloseDestroyDelay);
        }

        private void PlayIndicatorTrigger(GameObject indicator, string triggerName)
        {
            Animator animator = indicator.GetComponentInChildren<Animator>();
            if (animator == null) return;

            animator.SetTrigger(triggerName);
        }
    }
}
