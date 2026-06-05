using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    public partial class WorldManager : MonoBehaviourSingleton<WorldManager>
    {
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

            // CreatureData에 지정된 프리팹을 우선 사용하고, 없으면 기본 몬스터 프리팹을 사용합니다.
            CreatureController monsterPf = monsterData.creaturePf.GetComponent<CreatureController>();
            if (monsterPf == null)
            {
                Debug.LogWarning($"SpawnMonster failed. Monster prefab is not assigned. CreatureData: {monsterData?.name}");
                return;
            }

            MonsterController monsterController = Instantiate(monsterPf, worldPos, Quaternion.identity) as MonsterController;
            if (monsterController == null)
            {
                Debug.LogWarning($"SpawnMonster failed. MonsterController not found. Prefab: {monsterPf.name}");
                return;
            }
            monsterController.Model.Init(cellPos, monsterData);

            // 생성된 몬스터를 월드 조회 테이블에 등록합니다.
            creatures.Add(monsterController.GetInstanceID(), monsterController);
        }

        /// <summary>
        /// NPC 프리팹을 실제로 인스턴스화하고 모델 데이터를 초기화합니다.
        /// </summary>
        private void SpawnNPCInternal(CreatureData npcData, Vector3Int cellPos)
        {
            // Ground 타일이 없는 CellPos에는 NPC를 생성하지 않습니다.
            if (!TryGetMapWorldPosInternal(cellPos, out Vector3 worldPos)) return;

            if (npcData == null || npcData.creaturePf == null)
            {
                Debug.LogWarning($"SpawnNPC failed. NPC prefab is not assigned. CreatureData: {npcData?.name}");
                return;
            }

            // CreatureData에 지정된 NPC 프리팹을 생성하고 모델 데이터를 초기화합니다.
            CreatureController npcPf = npcData.creaturePf.GetComponent<CreatureController>();
            if (npcPf == null)
            {
                Debug.LogWarning($"SpawnNPC failed. CreatureController not found. Prefab: {npcData.creaturePf.name}");
                return;
            }

            NPCController npcController = Instantiate(npcPf, worldPos, Quaternion.identity) as NPCController;
            if (npcController == null)
            {
                Debug.LogWarning($"SpawnNPC failed. NPCController not found. Prefab: {npcPf.name}");
                return;
            }
            npcController.Model.Init(cellPos, npcData);

            // 생성된 NPC를 월드 조회 테이블에 등록합니다.
            creatures.Add(npcController.GetInstanceID(), npcController);
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
        /// 타일 삭제
        /// </summary>
        private void DespawnInternal(Vector3Int cellPos)
        {
            Destroy(tiles[cellPos].gameObject);
            tiles.Remove(cellPos);
        }

        /// <summary>
        /// 현재 월드에 등록된 모든 크리처를 제거합니다.
        /// </summary>
        private void DespawnAllCreatures()
        {
            foreach (KeyValuePair<int, CreatureController> pair in creatures)
            {
                if (pair.Value == null) continue;

                Destroy(pair.Value.gameObject);
            }

            creatures.Clear();
        }

        /// <summary>
        /// MapData에 저장된 몬스터 배치 정보를 기준으로 초기 몬스터를 생성합니다.
        /// </summary>
        private void SpawnMonstersInternal(MapData mapData)
        {
            if (mapData == null) return;

            foreach (MapMonsterSpawnData monsterSpawn in mapData.MonsterSpawns)
            {
                CreatureData monsterData = ResourceManager.GetResource(monsterSpawn.MonsterDataReference);
                if (monsterData == null)
                {
                    Debug.LogWarning($"SpawnMonsters skipped. Monster data is not loaded. RuntimeKey: {monsterSpawn.MonsterDataReference?.RuntimeKey}");
                    continue;
                }

                if (HasCreatureInCellPos(monsterSpawn.CellPos))
                {
                    Debug.LogWarning($"SpawnMonsters skipped. CellPos already occupied: {monsterSpawn.CellPos}");
                    continue;
                }

                if (!mapData.HasTile(monsterSpawn.CellPos))
                {
                    Debug.LogWarning($"SpawnMonsters skipped. Tile not found. CellPos: {monsterSpawn.CellPos}");
                    continue;
                }

                SpawnMonsterInternal(monsterData, monsterSpawn.CellPos);
            }
        }
    }
}
