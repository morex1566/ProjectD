using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TRPG.Runtime
{
    /// <summary>
    /// 월드 타일, 크리처 스폰, 점유 상태, 이동 범위 표시를 관리합니다.
    /// </summary>
    public partial class WorldManager : MonoBehaviourSingleton<WorldManager>
    {
        private static WorldManagerSettingsData settings = null;


        [Header(nameof(WorldManager) + ".Runtime")]

        [SerializeField, ReadOnly] private MapData currMapData = null;

        [SerializeField, ReadOnly] private GameObject currMap = null;

        [SerializeField, ReadOnly] private Dictionary<Vector3Int, TileController> tiles = new();

        [SerializeField, ReadOnly] private Dictionary<int, CreatureController> creatures = new();

        [SerializeField, ReadOnly] private Dictionary<Vector3Int, TileIndicator> tileIndicators = new();



        private void Awake()
        {
            Init();

            tiles = new Dictionary<Vector3Int, TileController>();
            creatures = new Dictionary<int, CreatureController>();
            tileIndicators = new Dictionary<Vector3Int, TileIndicator>();
        }

        public static void Init()
        {
            GetInstance();
            settings = Resources.Load<WorldManagerSettingsData>("SO_WorldManagerSettings");
        }



        /// <summary>
        /// 월드 좌표에 대응하는 Ground CellPos를 반환합니다.
        /// 타일 크기는 1이므로 WorldPosition (0, 0, 0)은 CellPos (0, 0)에 대응합니다.
        /// </summary>
        public bool TryGetMapCellPos(Vector3 worldPos, out Vector3Int cellPos)
        {
            cellPos = WorldToCellPos(worldPos);

            return tiles.ContainsKey(cellPos);
        }

        /// <summary>
        /// Ground CellPos가 유효하면 월드 중심 좌표를 반환합니다.
        /// 타일 크기는 1이므로 CellPos (0, 0)은 WorldPosition (0, 0, 0)에 대응합니다.
        /// </summary>
        public bool TryGetMapWorldPos(Vector3Int cellPos, out Vector3 worldPos)
        {
            if (!tiles.ContainsKey(cellPos))
            {
                worldPos = default;
                return false;
            }

            worldPos = CellPosToWorldPos(cellPos);
            return true;
        }

        public static Vector3Int WorldToCellPos(Vector3 worldPos)
        {
            // 타일 크기는 1입니다. WorldPosition (0, 0, 0)은 CellPos (0, 0)에 매핑되고 z는 논리 좌표에서 사용하지 않습니다.
            return new Vector3Int(
                Mathf.FloorToInt(worldPos.x + 0.5f),
                Mathf.FloorToInt(worldPos.y + 0.5f),
                0);
        }

        public static Vector3 CellPosToWorldPos(Vector3Int cellPos)
        {
            return new Vector3(cellPos.x, cellPos.y, cellPos.z);
        }

        /// <summary>
        /// originCellPos 기준 현재 맵에서 이동가능한 CellPos를 가져옵니다.
        /// </summary>
        public List<Vector3Int> GetMovableCellPosList(Vector3Int originCellPos, List<Vector3Int> directions, bool isRepeatable, bool isIncludeCreature)
        {
            List<Vector3Int> movableCellPosList = new();

            if (directions == null) return movableCellPosList;

            foreach (Vector3Int direction in directions)
            {
                if (direction == Vector3Int.zero) continue;

                Vector3Int candidateCellPos = originCellPos + direction;
                while (TryGetMapWorldPos(candidateCellPos, out _))
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
        public bool HasIndicatorInCellPos(Vector3Int cellPos, CreatureController owner)
        {
            return tileIndicators.ContainsKey(cellPos) && tileIndicators[cellPos].Owner == owner;
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
        public bool HasMonsterInWorldPos(Vector3 worldPos, out MonsterController monsterController)
        {
            if (!TryGetMapCellPos(worldPos, out Vector3Int cellPos))
            {
                monsterController = null;
                return false;
            }

            // 타일 기반 클릭 판정은 스프라이트 bounds가 아니라 점유 CellPos를 기준으로 합니다.
            return HasMonsterInCellPos(cellPos, out monsterController);
        }

        /// <summary>
        /// 위치에 있는 Creature들을 리턴
        /// </summary>
        public List<CreatureController> GetCreaturesInCellPosList(List<Vector3Int> cellPosList)
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




        public void SpawnMonster(CreatureData monsterData, Vector3Int cellPos)
        {
            // Ground 타일이 없는 CellPos에는 몬스터를 생성하지 않습니다.
            if (!TryGetMapWorldPos(cellPos, out Vector3 worldPos)) return;

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

        public void SpawnPlayer(Vector3Int cellPos)
        {
            // Ground 타일이 없는 CellPos에는 플레이어를 생성하지 않습니다.
            if (!TryGetMapWorldPos(cellPos, out Vector3 worldPos)) return;

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
        public void Despawn(int instanceId)
        {
            Destroy(creatures[instanceId].gameObject);
            creatures.Remove(instanceId);
        }

        /// <summary>
        /// 타일에 표식 넣기
        /// </summary>
        public void AddAllyTileIndicator(List<Vector3Int> cellPosList, CreatureController owner)
        {
            RemoveTileIndicators(owner);

            foreach (Vector3Int cellPos in cellPosList)
            {
                if (!TryGetMapWorldPos(cellPos, out Vector3 indicatorWorldPos)) continue;

                RemoveTileIndicator(cellPos);

                TileIndicator allyTileIndicatorPb = settings.AllyTileIndicatorPb;
                TileIndicator tileIndicator = Instantiate(allyTileIndicatorPb, indicatorWorldPos, Quaternion.identity, transform);
                tileIndicator.Init(owner, cellPos);
                tileIndicators.Add(cellPos, tileIndicator);
            }
        }

        public void AddEnemyTileIndicator(List<Vector3Int> cellPosList, CreatureController owner)
        {
            RemoveTileIndicators(owner);

            foreach (Vector3Int cellPos in cellPosList)
            {
                if (!TryGetMapWorldPos(cellPos, out Vector3 indicatorWorldPos)) continue;

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
        public void RemoveTileIndicators(CreatureController owner)
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

        private void RemoveTileIndicator(Vector3Int cellPos)
        {
            if (!tileIndicators.TryGetValue(cellPos, out TileIndicator tileIndicator)) return;

            // dictionary entry와 Scene object를 함께 제거해야 다음 indicator 생성 시 key가 충돌하지 않습니다.
            if (tileIndicator != null) Destroy(tileIndicator.gameObject);

            tileIndicators.Remove(cellPos);
        }



        private void LoadMapData(MapData mapData)
        {
            if (mapData == null) return;

            UnloadMapData();
            currMapData = mapData;

            int topRowCellY = GetTopRowCellY(mapData.Tiles);
            currMap = new GameObject("Map");

            // 타일 데이터를 읽어와서 월드에 인스턴싱
            foreach (MapTileData tileData in mapData.Tiles)
            {
                if (tileData.TilePb == null) continue;

                TileController tile = Instantiate(tileData.TilePb, CellPosToWorldPos(tileData.CellPos), Quaternion.identity, currMap.transform);

                ApplyTileOrderInLayer(tile, topRowCellY - tileData.CellPos.y);
                tiles.Add(tileData.CellPos, tile);
            }
        }

        private void UnloadMapData()
        {
            foreach (KeyValuePair<int, CreatureController> pair in creatures)
            {
                if (pair.Value == null) continue;

                DestroyRuntimeObject(pair.Value.gameObject);
            }

            foreach (KeyValuePair<Vector3Int, TileIndicator> pair in tileIndicators)
            {
                if (pair.Value == null) continue;

                DestroyRuntimeObject(pair.Value.gameObject);
            }

            if (currMap != null)
            {
                DestroyRuntimeObject(currMap);
            }

            currMapData = null;
            currMap = null;
            tiles.Clear();
            creatures.Clear();
            tileIndicators.Clear();
        }

        private static void DestroyRuntimeObject(GameObject target)
        {
            if (target == null) return;

#if UNITY_EDITOR
            // 에디터 메뉴에서 테스트 맵을 교체할 때도 Scene 오브젝트가 즉시 정리되어야 합니다.
            if (!Application.isPlaying)
            {
                DestroyImmediate(target);
                return;
            }
#endif

            Destroy(target);
        }

        /// <summary>
        /// 타일 밑에 보면 음영이 있는데 이부분이 가려지도록 설계하기 위해
        /// </summary>
        private static int GetTopRowCellY(IReadOnlyList<MapTileData> tileDataList)
        {
            if (tileDataList.Count == 0) return 0;

            int topRowCellY = tileDataList[0].CellPos.y;
            foreach (MapTileData tileData in tileDataList)
            {
                topRowCellY = Mathf.Max(topRowCellY, tileData.CellPos.y);
            }

            return topRowCellY;
        }

        /// <summary>
        /// CellPos y가 큰 최상단 행부터 SpriteRenderer Order in Layer를 0, 1, 2...로 배정합니다.
        /// 타일 밑에 보면 음영이 있는데 이부분이 가려지도록 설계하기 위해
        /// </summary>
        private static void ApplyTileOrderInLayer(TileController tile, int baseOrderInLayer)
        {
            SpriteRenderer[] renderers = tile.GetComponentsInChildren<SpriteRenderer>(true);
            if (renderers.Length == 0) return;

            int minOrderInLayer = renderers[0].sortingOrder;
            foreach (SpriteRenderer renderer in renderers)
            {
                minOrderInLayer = Mathf.Min(minOrderInLayer, renderer.sortingOrder);
            }

            foreach (SpriteRenderer renderer in renderers)
            {
                int relativeOrderInLayer = renderer.sortingOrder - minOrderInLayer;
                renderer.sortingOrder = baseOrderInLayer + relativeOrderInLayer;
            }
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
            settings = Resources.Load<WorldManagerSettingsData>("SO_WorldManagerSettings");
            var awaiter = ResourceManager.LoadAsync(UnityConstant.Addressable.Label.Core).GetAwaiter();

            GetInstance().UnloadMapData();

            awaiter.OnCompleted(() =>
            {
                MapData testMapData = ResourceManager.GetResource(settings.TestMapData);          
                WorldManager.GetInstance().LoadMapData(testMapData);
            });
        }
    }

#endif
}
