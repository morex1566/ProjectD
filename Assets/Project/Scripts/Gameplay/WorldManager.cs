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

        [SerializeField, ReadOnly] private List<CreatureController> creatures = null;

        [Header("Setup")]

        [SerializeField] private GameObject monsterPb = null;
        [SerializeField] private GameObject playerPb = null;

        private void OnValidate()
        {
            Init();
        }

        private void Awake()
        {
            Init();

            // 인스턴싱
            InstantiateMonster(ResourceManager.GetMonsterData("Monster_00"), new Vector3Int(0, 2, 0));
            InstantiatePlayer(new Vector3Int(0, 0, 0));
        }

        private void Init()
        {
            tilemaps = new List<Tilemap>();
            ground = new List<Tilemap>();

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

        public void InstantiateMonster(MonsterData monsterData, Vector3Int cellPos)
        {
            // 인스턴싱할 수 없는 위치
            if (!TryGetGroundWorldPosition(cellPos, out Vector3 worldPos)) return;

            GameObject monsterInst = Instantiate(monsterPb, worldPos, Quaternion.identity);
            {
                MonsterController monsterController = monsterInst.GetComponent<MonsterController>();
                monsterController.Model.Init(monsterData, cellPos);
                creatures.Add(monsterController);
            }
        }

        public void InstantiatePlayer(Vector3Int cellPos)
        {
            // 인스턴싱할 수 없는 위치
            if (!TryGetGroundWorldPosition(cellPos, out Vector3 worldPos)) return;

            GameObject playerInst = Instantiate(playerPb, worldPos, Quaternion.identity);
            {
                PlayerController playerController = playerInst.GetComponent<PlayerController>();
                creatures.Add(playerController);
            }
        }

        /// <summary>
        /// 이 위치에 몬스터가 있는지 확인합니다. 
        /// </summary>
        public bool HasMonster(Vector3Int cellPos, out MonsterController monster)
        {
            monster = null;

            foreach (CreatureController creature in creatures)
            {
                // 위치에 creature가 없음
                if (cellPos != creature.Model.CellPos) continue;

                // creature가 monster가 아님
                if (creature is not MonsterController foundMonster) continue;

                monster = foundMonster;
                return true;
            }

            return false;
        }
    }
}
