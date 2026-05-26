using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TRPG.Runtime
{
    /// <summary>
    /// 월드 타일맵, 크리처 스폰, 점유 상태, 이동 범위 표시를 관리합니다.
    /// </summary>
    public partial class WorldManager : MonoBehaviourSingleton<WorldManager>
    {
        [Header("Runtime")]

        [SerializeField, ReadOnly] private List<Tilemap> tilemaps = null;

        /// <summary>
        /// Creature가 이동할 수 있는 Tilemap
        /// </summary>
        [SerializeField, ReadOnly] private List<Tilemap> ground = null;

        [SerializeField, ReadOnly] private Dictionary<int, CreatureController> creatures = null;

        private Dictionary<Vector3Int, GameObject> tileIndicators = new();

        [Header("Setup")]

        [SerializeField] private GameObject monsterPb = null;

        [SerializeField] private GameObject playerPb = null;

        [SerializeField] private GameObject allyMovableTilePb = null;

        [SerializeField] private GameObject enemyMovableTilePb = null;

        public GameObject AllyMovableTilePb => allyMovableTilePb;

        public GameObject EnemyMovableTilePb => enemyMovableTilePb;

        private void OnValidate()
        {
            Init();
        }

        private void Awake()
        {
            Init();

            ResourceManager.Database.Load();

            // 임시 전투 배치를 생성합니다.
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
        public bool TryGetGroundCellPos(Vector3 worldPos, out Vector3Int cellPos)
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
        public bool TryGetGroundWorldPos(Vector3Int cellPos, out Vector3 worldPos)
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
            // Ground 타일이 없는 셀에는 몬스터를 생성하지 않습니다.
            if (!TryGetGroundWorldPos(cellPos, out Vector3 worldPos)) return;

            // 몬스터 프리팹을 생성하고 모델 데이터를 초기화합니다.
            GameObject monsterInst = Instantiate(monsterPb, worldPos, Quaternion.identity);
            MonsterController monsterController = monsterInst.GetComponent<MonsterController>();
            if (monsterController == null)
            {
                Debug.LogWarning($"SpawnMonster failed. MonsterController not found. Prefab: {monsterPb.name}");
                Destroy(monsterInst);
                return;
            }
            monsterController.Model.Init(cellPos, monsterData);

            // 생성된 몬스터를 월드 조회 테이블에 등록합니다.
            creatures.Add(monsterController.GetInstanceID(), monsterController);
        }

        public void SpawnPlayer(Vector3Int cellPos)
        {
            // Ground 타일이 없는 셀에는 플레이어를 생성하지 않습니다.
            if (!TryGetGroundWorldPos(cellPos, out Vector3 worldPos)) return;

            // 플레이어 프리팹을 생성하고 모델 데이터를 초기화합니다.
            GameObject playerInst = Instantiate(playerPb, worldPos, Quaternion.identity);
            PlayerController playerController = playerInst.GetComponent<PlayerController>();
            if (playerController == null)
            {
                Debug.LogWarning($"SpawnPlayer failed. PlayerController not found. Prefab: {playerPb.name}");
                Destroy(playerInst);
                return;
            }
            playerController.Model.Init(cellPos);

            // 생성된 플레이어를 월드 조회 테이블에 등록합니다.
            creatures.Add(playerController.GetInstanceID(), playerController);
        }

        /// <summary>
        /// 크리처 삭제
        /// </summary>
        public void Despawn(int instanceId)
        {
            Destroy(creatures[instanceId].gameObject);
            creatures.Remove(instanceId);
        }

        /// <summary>
        /// 현재 표시 중인 이동 가능 타일 표시를 모두 제거합니다.
        /// </summary>
        public void ClearMoveRange()
        {
            RemoveTileIndicators(tileIndicators);
        }

        /// <summary>
        /// 내가 소유한 타일 인디케이터인지?
        /// </summary>
        public bool IsMovableHighlighted(Vector3Int cellPos)
        {
            return tileIndicators.ContainsKey(cellPos);
        }

        /// <summary>
        /// 이 위치에 몬스터가 있는지 확인합니다. 
        /// </summary>
        public bool HasMonsterInCellPos(Vector3Int cellPos, out MonsterController monsterController)
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

        /// <summary>
        /// 이 위치에 몬스터가 있는지 확인합니다. 
        /// </summary>
        public bool HasMonsterInWorldPos(Vector3 worldPos, out MonsterController monsterController)
        {
            if (!TryGetGroundCellPos(worldPos, out Vector3Int cellPos))
            {
                monsterController = null;
                return false;
            }

            // 타일 기반 클릭 판정은 스프라이트 bounds가 아니라 점유 셀을 기준으로 합니다.
            return HasMonsterInCellPos(cellPos, out monsterController);
        }

        /// <summary>
        /// 타일에 표식 넣기
        /// </summary>
        public void AddTileIndicator(GameObject prefab, Vector3Int cellPos, Vector3 worldPos)
        {
            if (prefab == null)
            {
                Debug.LogWarning("Move range tileIndicator prefab is not assigned.");
                return;
            }

            GameObject tileIndicator = Instantiate(prefab, worldPos, Quaternion.identity, transform);
            tileIndicators.Add(cellPos, tileIndicator);
            PlayTileIndicatorTrigger(tileIndicator, UnityConstant.Animator.Parameters.AC_TIleIndicator.Trigger.OnOpen);
        }

        /// <summary>
        /// 인디케이터 삭제
        /// </summary>
        private void RemoveTileIndicators(Dictionary<Vector3Int, GameObject> indicators)
        {
            foreach (KeyValuePair<Vector3Int, GameObject> pair in indicators)
            {
                if (pair.Value == null) continue;

                // 인디케이터 삭제
                // TODO : 애니메이션 끝에 맞춰서 삭제해야할듯
                PlayTileIndicatorTrigger(pair.Value, UnityConstant.Animator.Parameters.AC_TIleIndicator.Trigger.OnClose);
                Destroy(pair.Value, IndicatorCloseDestroyDelay);
            }

            indicators.Clear();
        }
    }

    /// <summary>
    /// 뷰
    /// </summary>
    public partial class WorldManager : MonoBehaviourSingleton<WorldManager>
    {
        private void PlayTileIndicatorTrigger(GameObject indicator, string triggerName)
        {
            Animator animator = indicator.GetComponentInChildren<Animator>();
            if (animator == null) return;

            animator.SetTrigger(triggerName);
        }
    }

    /// <summary>
    /// const
    /// </summary>
    public partial class WorldManager : MonoBehaviourSingleton<WorldManager>
    {
        private const float IndicatorCloseDestroyDelay = 0.35f;
    }
}
