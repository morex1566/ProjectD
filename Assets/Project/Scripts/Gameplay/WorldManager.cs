using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TRPG.Runtime
{
    /// <summary>
    /// 월드 시스템 진입점으로서 맵, 크리처, 인디케이터 기능을 중개합니다.
    /// </summary>
    public partial class WorldManager : MonoBehaviourSingleton<WorldManager>
    {
        private static WorldManagerSettingsData settings = null;


        [Header(nameof(WorldManager) + ".Runtime")]

        [SerializeField, ReadOnly] private MapController currMapController = null;

        [SerializeField, ReadOnly] private Dictionary<int, CreatureController> creatures = new();

        [SerializeField, ReadOnly] private Dictionary<Vector3Int, TileIndicator> tileIndicators = new();



        public static Action<MapController> OnMapLoaded;



        private void Awake()
        {
            Init();

            creatures = new Dictionary<int, CreatureController>();
            tileIndicators = new Dictionary<Vector3Int, TileIndicator>();
        }

        /// <summary>
        /// 월드 매니저 인스턴스와 설정 데이터를 준비합니다.
        /// </summary>
        public static void Init()
        {
            GetInstance();
            settings = Resources.Load<WorldManagerSettingsData>("SO_WorldManagerSettings");
        }

        /// <summary>
        /// MapController 프리팹을 생성하고 맵 데이터 로드를 위임합니다.
        /// </summary>
        public static void LoadMapData(MapData mapData)
        {
            GetInstance().LoadMapDataInternal(mapData);
        }

        /// <summary>
        /// 현재 맵, 크리처, 타일 인디케이터 런타임 오브젝트를 모두 정리합니다.
        /// </summary>
        public static void UnloadMapData()
        {
            GetInstance().UnloadMapDataInternal();
        }



        /// <summary>
        /// 월드 좌표에 대응하는 유효한 맵 CellPos를 찾습니다.
        /// </summary>
        public static bool TryGetMapCellPos(Vector3 worldPos, out Vector3Int cellPos)
        {
            return GetInstance().TryGetMapCellPosInternal(worldPos, out cellPos);
        }

        /// <summary>
        /// 맵 CellPos에 대응하는 월드 중심 좌표를 찾습니다.
        /// </summary>
        public static bool TryGetMapWorldPos(Vector3Int cellPos, out Vector3 worldPos)
        {
            return GetInstance().TryGetMapWorldPosInternal(cellPos, out worldPos);
        }

        /// <summary>
        /// 기준 CellPos와 이동 방향 데이터로 현재 맵에서 이동 가능한 CellPos 목록을 계산합니다.
        /// </summary>
        public static List<Vector3Int> GetMovableCellPosList(Vector3Int originCellPos, List<Vector3Int> directions, bool isRepeatable, bool isIncludeCreature)
        {
            return GetInstance().GetMovableCellPosListInternal(originCellPos, directions, isRepeatable, isIncludeCreature);
        }

        /// <summary>
        /// 지정 CellPos에 owner가 소유한 타일 인디케이터가 있는지 확인합니다.
        /// </summary>
        public static bool HasIndicatorInCellPos(Vector3Int cellPos, CreatureController owner)
        {
            return GetInstance().HasIndicatorInCellPosInternal(cellPos, owner);
        }

        /// <summary>
        /// 지정 CellPos를 점유한 몬스터를 찾습니다.
        /// </summary>
        public static bool HasMonsterInCellPos(Vector3Int cellPos, out MonsterController monsterController)
        {
            return GetInstance().HasMonsterInCellPosInternal(cellPos, out monsterController);
        }

        /// <summary>
        /// 월드 좌표가 가리키는 CellPos를 점유한 몬스터를 찾습니다.
        /// </summary>
        public static bool HasMonsterInWorldPos(Vector3 worldPos, out MonsterController monsterController)
        {
            return GetInstance().HasMonsterInWorldPosInternal(worldPos, out monsterController);
        }

        /// <summary>
        /// 지정 CellPos 목록에 있는 모든 크리처를 반환합니다.
        /// </summary>
        public static List<CreatureController> GetCreaturesInCellPosList(List<Vector3Int> cellPosList)
        {
            return GetInstance().GetCreaturesInCellPosListInternal(cellPosList);
        }

        /// <summary>
        /// 지정 CellPos에 몬스터를 생성하고 월드 점유 목록에 등록합니다.
        /// </summary>
        public static void SpawnMonster(CreatureData monsterData, Vector3Int cellPos)
        {
            GetInstance().SpawnMonsterInternal(monsterData, cellPos);
        }

        /// <summary>
        /// 지정 CellPos에 플레이어를 생성하고 월드 점유 목록에 등록합니다.
        /// </summary>
        public static void SpawnPlayer(Vector3Int cellPos)
        {
            GetInstance().SpawnPlayerInternal(cellPos);
        }

        /// <summary>
        /// instanceId에 해당하는 크리처를 제거하고 월드 점유 목록에서 해제합니다.
        /// </summary>
        public static void Despawn(int instanceId)
        {
            GetInstance().DespawnInternal(instanceId);
        }

        /// <summary>
        /// owner의 기존 인디케이터를 지우고 아군 이동 범위 인디케이터를 표시합니다.
        /// </summary>
        public static void AddAllyTileIndicator(List<Vector3Int> cellPosList, CreatureController owner)
        {
            GetInstance().AddAllyTileIndicatorInternal(cellPosList, owner);
        }

        /// <summary>
        /// owner의 기존 인디케이터를 지우고 적 대상 범위 인디케이터를 표시합니다.
        /// </summary>
        public static void AddEnemyTileIndicator(List<Vector3Int> cellPosList, CreatureController owner)
        {
            GetInstance().AddEnemyTileIndicatorInternal(cellPosList, owner);
        }

        /// <summary>
        /// owner가 소유한 모든 타일 인디케이터를 제거합니다.
        /// </summary>
        public static void RemoveTileIndicators(CreatureController owner)
        {
            GetInstance().RemoveTileIndicatorsInternal(owner);
        }



        /// <summary>
        /// 월드 좌표에 대응하는 Ground CellPos를 반환합니다.
        /// 타일 크기는 1이므로 WorldPosition (0, 0, 0)은 CellPos (0, 0)에 대응합니다.
        /// </summary>
        private bool TryGetMapCellPosInternal(Vector3 worldPos, out Vector3Int cellPos)
        {
            if (currMapController != null)
            {
                return currMapController.TryGetMapCellPos(worldPos, out cellPos);
            }

            cellPos = default;
            return false;
        }

        /// <summary>
        /// Ground CellPos가 유효하면 월드 중심 좌표를 반환합니다.
        /// 타일 크기는 1이므로 CellPos (0, 0)은 WorldPosition (0, 0, 0)에 대응합니다.
        /// </summary>
        private bool TryGetMapWorldPosInternal(Vector3Int cellPos, out Vector3 worldPos)
        {
            if (currMapController != null)
            {
                return currMapController.TryGetMapWorldPos(cellPos, out worldPos);
            }

            worldPos = default;
            return false;
        }

        /// <summary>
        /// 월드 좌표를 논리 CellPos로 변환합니다.
        /// </summary>
        public static Vector3Int WorldToCellPos(Vector3 worldPos)
        {
            return MapController.WorldToCellPos(worldPos);
        }

        /// <summary>
        /// 논리 CellPos를 월드 중심 좌표로 변환합니다.
        /// </summary>
        public static Vector3 CellPosToWorldPos(Vector3Int cellPos)
        {
            return MapController.CellPosToWorldPos(cellPos);
        }

        /// <summary>
        /// originCellPos 기준 현재 맵에서 이동가능한 CellPos를 가져옵니다.
        /// </summary>
        private List<Vector3Int> GetMovableCellPosListInternal(Vector3Int originCellPos, List<Vector3Int> directions, bool isRepeatable, bool isIncludeCreature)
        {
            List<Vector3Int> movableCellPosList = new();

            if (directions == null) return movableCellPosList;

            foreach (Vector3Int direction in directions)
            {
                if (direction == Vector3Int.zero) continue;

                Vector3Int candidateCellPos = originCellPos + direction;
                while (TryGetMapWorldPosInternal(candidateCellPos, out _))
                {
                    // 다른 크리처가 점유한 CellPos는 이동 가능 목록에서 제외하고, 반복 이동도 그 지점에서 멈춥니다.
                    if (!isIncludeCreature && HasCreatureInCellPos(candidateCellPos)) break;

                    if (movableCellPosList.Contains(candidateCellPos)) break;

                    movableCellPosList.Add(candidateCellPos);

                    if (!isRepeatable) break;

                    candidateCellPos += direction;
                }
            }

            return movableCellPosList;
        }



        /// <summary>
        /// 내가 소유한 타일 인디케이터인지?
        /// </summary>
        private bool HasIndicatorInCellPosInternal(Vector3Int cellPos, CreatureController owner)
        {
            return tileIndicators.ContainsKey(cellPos) && tileIndicators[cellPos].Owner == owner;
        }

        /// <summary>
        /// 이 위치에 몬스터가 있는지 확인합니다.
        /// </summary>
        private bool HasMonsterInCellPosInternal(Vector3Int cellPos, out MonsterController monsterController)
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
        /// 이 위치에 크리쳐가 있는지 확인합니다.
        /// </summary>
        private bool HasCreatureInCellPos(Vector3Int cellPos)
        {
            foreach (KeyValuePair<int, CreatureController> pair in creatures)
            {
                if (cellPos != pair.Value.Model.CellPos) continue;

                return true;
            }

            return false;
        }

        /// <summary>
        /// 이 위치에 몬스터가 있는지 확인합니다.
        /// </summary>
        private bool HasMonsterInWorldPosInternal(Vector3 worldPos, out MonsterController monsterController)
        {
            if (!TryGetMapCellPosInternal(worldPos, out Vector3Int cellPos))
            {
                monsterController = null;
                return false;
            }

            // 타일 기반 클릭 판정은 스프라이트 bounds가 아니라 점유 CellPos를 기준으로 합니다.
            return HasMonsterInCellPosInternal(cellPos, out monsterController);
        }

        /// <summary>
        /// 위치에 있는 Creature들을 리턴
        /// </summary>
        private List<CreatureController> GetCreaturesInCellPosListInternal(List<Vector3Int> cellPosList)
        {
            List<CreatureController> results = new();

            foreach (Vector3Int cellPos in cellPosList)
            {
                foreach (KeyValuePair<int, CreatureController> pair in creatures)
                {
                    if (!(cellPos == pair.Value.Model.CellPos)) continue;

                    results.Add(pair.Value);
                }
            }

            return results;
        }




        /// <summary>
        /// 몬스터 프리팹을 실제로 인스턴스화하고 모델 데이터를 초기화합니다.
        /// </summary>
        private void SpawnMonsterInternal(CreatureData monsterData, Vector3Int cellPos)
        {
            // Ground 타일이 없는 CellPos에는 몬스터를 생성하지 않습니다.
            if (!TryGetMapWorldPosInternal(cellPos, out Vector3 worldPos)) return;

            // 몬스터 프리팹을 생성하고 모델 데이터를 초기화합니다.
            CreatureController monsterPb = settings.MonsterPb;
            MonsterController monsterController = Instantiate(monsterPb, worldPos, Quaternion.identity) as MonsterController;
            if (monsterController == null)
            {
                Debug.LogWarning($"SpawnMonster failed. MonsterController not found. Prefab: {monsterPb.name}");
                return;
            }
            monsterController.Model.Init(cellPos, monsterData);

            // 생성된 몬스터를 월드 조회 테이블에 등록합니다.
            creatures.Add(monsterController.GetInstanceID(), monsterController);
        }

        /// <summary>
        /// 플레이어 프리팹을 실제로 인스턴스화하고 모델 데이터를 초기화합니다.
        /// </summary>
        private void SpawnPlayerInternal(Vector3Int cellPos)
        {
            // Ground 타일이 없는 CellPos에는 플레이어를 생성하지 않습니다.
            if (!TryGetMapWorldPosInternal(cellPos, out Vector3 worldPos)) return;

            // 플레이어 프리팹을 생성하고 모델 데이터를 초기화합니다.
            CreatureController playerPb = settings.PlayerPb;
            PlayerController playerController = Instantiate(playerPb, worldPos, Quaternion.identity) as PlayerController;
            if (playerController == null)
            {
                Debug.LogWarning($"SpawnPlayer failed. PlayerController not found. Prefab: {playerPb.name}");
                return;
            }
            playerController.Model.Init(cellPos);

            // 생성된 플레이어를 월드 조회 테이블에 등록합니다.
            creatures.Add(playerController.GetInstanceID(), playerController);
        }

        /// <summary>
        /// 크리처 삭제
        /// </summary>
        private void DespawnInternal(int instanceId)
        {
            Destroy(creatures[instanceId].gameObject);
            creatures.Remove(instanceId);
        }

        /// <summary>
        /// 타일에 표식 넣기
        /// </summary>
        private void AddAllyTileIndicatorInternal(List<Vector3Int> cellPosList, CreatureController owner)
        {
            RemoveTileIndicatorsInternal(owner);

            foreach (Vector3Int cellPos in cellPosList)
            {
                if (!TryGetMapWorldPosInternal(cellPos, out Vector3 indicatorWorldPos)) continue;

                RemoveTileIndicator(cellPos);

                TileIndicator allyTileIndicatorPb = settings.AllyTileIndicatorPb;
                TileIndicator tileIndicator = Instantiate(allyTileIndicatorPb, indicatorWorldPos, Quaternion.identity, transform);
                tileIndicator.Init(owner, cellPos);
                tileIndicators.Add(cellPos, tileIndicator);
            }
        }

        /// <summary>
        /// 적 대상 범위 CellPos마다 타일 인디케이터를 생성합니다.
        /// </summary>
        private void AddEnemyTileIndicatorInternal(List<Vector3Int> cellPosList, CreatureController owner)
        {
            RemoveTileIndicatorsInternal(owner);

            foreach (Vector3Int cellPos in cellPosList)
            {
                if (!TryGetMapWorldPosInternal(cellPos, out Vector3 indicatorWorldPos)) continue;

                RemoveTileIndicator(cellPos);

                TileIndicator enemyTileIndicatorPb = settings.EnemyTileIndicatorPb;
                TileIndicator tileIndicator = Instantiate(enemyTileIndicatorPb, indicatorWorldPos, Quaternion.identity, transform);
                tileIndicator.Init(owner, cellPos);
                tileIndicators.Add(cellPos, tileIndicator);
            }
        }

        /// <summary>
        /// 인디케이터 삭제
        /// </summary>
        private void RemoveTileIndicatorsInternal(CreatureController owner)
        {
            List<Vector3Int> removeCellPosList = new();
            foreach (KeyValuePair<Vector3Int, TileIndicator> pair in tileIndicators)
            {
                if (pair.Value == null)
                {
                    removeCellPosList.Add(pair.Key);
                    continue;
                }

                if (!(pair.Value.Owner == owner)) continue;

                removeCellPosList.Add(pair.Key);
            }

            foreach (Vector3Int cellPos in removeCellPosList)
            {
                RemoveTileIndicator(cellPos);
            }
        }

        /// <summary>
        /// 지정 CellPos의 인디케이터 오브젝트와 조회 항목을 함께 제거합니다.
        /// </summary>
        private void RemoveTileIndicator(Vector3Int cellPos)
        {
            if (!tileIndicators.TryGetValue(cellPos, out TileIndicator tileIndicator)) return;

            // dictionary entry와 Scene object를 함께 제거해야 다음 indicator 생성 시 key가 충돌하지 않습니다.
            if (tileIndicator != null) Destroy(tileIndicator.gameObject);

            tileIndicators.Remove(cellPos);
        }



        /// <summary>
        /// MapController 프리팹을 생성하고 맵 데이터 로드를 위임합니다.
        /// </summary>
        private void LoadMapDataInternal(MapData mapData)
        {
            if (mapData == null) return;

            UnloadMapDataInternal();
            if (settings.MapPb == null)
            {
                Debug.LogWarning("LoadMapData failed. MapController prefab is not assigned.");
                return;
            }

            // WorldManager는 MapController 인스턴스를 소유하고, 맵 관련 처리는 MapController에 위임합니다.
            currMapController = Instantiate(settings.MapPb, transform);
            currMapController.LoadMapData(mapData);
            OnMapLoaded?.Invoke(currMapController);
        }

        /// <summary>
        /// 현재 맵, 크리처, 타일 인디케이터 런타임 오브젝트를 모두 정리합니다.
        /// </summary>
        private void UnloadMapDataInternal()
        {
            foreach (KeyValuePair<int, CreatureController> pair in creatures)
            {
                if (pair.Value == null) continue;

                Destroy(pair.Value.gameObject);
            }

            foreach (KeyValuePair<Vector3Int, TileIndicator> pair in tileIndicators)
            {
                if (pair.Value == null) continue;

                Destroy(pair.Value.gameObject);
            }

            if (currMapController != null)
            {
                currMapController.UnloadMapData();
                Destroy(currMapController.gameObject);
            }

            currMapController = null;
            creatures.Clear();
            tileIndicators.Clear();
        }
    }

    public partial class WorldManager : MonoBehaviourSingleton<WorldManager>
    {
        public static readonly Vector3 TileSize = Vector3.one;

        public static class BackgroundColor
        {
            public static readonly string Sky = "#1E202A";

            public static readonly string Stone = "#1E1E1E";
        }
    }

#if UNITY_EDITOR
    public partial class WorldManager : MonoBehaviourSingleton<WorldManager>
    {
        /// <summary>
        /// WorldManagerSettings의 TestMapData를 로드
        /// </summary>
        [MenuItem("TRPG/WorldManager/LoadTestMapData()")]
        private static void LoadTestMapData()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("LoadTestMapData is only available in Play Mode.");
                return;
            }

            settings = Resources.Load<WorldManagerSettingsData>("SO_WorldManagerSettings");
            var awaiter = ResourceManager.LoadAsync(UnityConstant.Addressable.Label.Core).GetAwaiter();

            UnloadMapData();

            awaiter.OnCompleted(() =>
            {
                if (!Application.isPlaying) return;

                MapData testMapData = ResourceManager.GetResource(settings.TestMapData);          
                LoadMapData(testMapData);
            });
        }

        /// <summary>
        /// 플레이 모드에서만 테스트 맵 로드 메뉴를 활성화합니다.
        /// </summary>
        [MenuItem("TRPG/WorldManager/LoadTestMapData()", true)]
        private static bool CanLoadTestMapData()
        {
            return Application.isPlaying;
        }
    }

#endif
}
