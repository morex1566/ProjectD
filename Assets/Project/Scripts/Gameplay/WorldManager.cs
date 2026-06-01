using System;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 월드 시스템 진입점으로서 맵, 크리처, 인디케이터 기능을 중개합니다.
    /// </summary>
    public partial class WorldManager : MonoBehaviourSingleton<WorldManager>
    {
        private static WorldManagerSettingsData settings = null;


        [Header(nameof(WorldManager) + ".Runtime")]

        [SerializeField, ReadOnly] private MapData currMapData = null;

        [SerializeField, ReadOnly] private Transform mapRoot = null;

        [SerializeField, ReadOnly] private Dictionary<Vector3Int, TileController> tiles = new();

        [SerializeField, ReadOnly] private Dictionary<int, CreatureController> creatures = new();

        [SerializeField, ReadOnly] private Dictionary<Vector3Int, TileIndicator> tileIndicators = new();



        public static Action OnMapLoaded;



        private void Awake()
        {
            Init();

            tiles = new Dictionary<Vector3Int, TileController>();
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
        /// 지정 MapData를 현재 월드에 로드합니다.
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
        /// 현재 로드된 맵의 월드 기준 정중앙 좌표를 찾습니다.
        /// </summary>
        public static bool TryGetMapCenterWorldPos(out Vector3 worldPos)
        {
            return GetInstance().TryGetMapCenterWorldPosInternal(out worldPos);
        }

        /// <summary>
        /// 현재 로드된 맵의 열 개수를 반환합니다.
        /// </summary>
        public static int GetMapColumnCount()
        {
            return GetInstance().GetMapColumnCountInternal();
        }

        /// <summary>
        /// 현재 로드된 맵의 행 개수를 반환합니다.
        /// </summary>
        public static int GetMapRowCount()
        {
            return GetInstance().GetMapRowCountInternal();
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
    }
}
