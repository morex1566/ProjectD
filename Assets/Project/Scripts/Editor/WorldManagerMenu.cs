using TRPG.Runtime;
using UnityEditor;
using UnityEngine;

namespace TRPG.Editor
{
    public static class WorldManagerMenu
    {
        [MenuItem("TRPG/Gameplay/Move Initial Combatants")]
        private static void MoveInitialCombatants()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("Move Initial Combatants는 Play Mode에서만 실행할 수 있습니다.");
                return;
            }

            WorldManager worldManager = Object.FindAnyObjectByType<WorldManager>();
            if (worldManager == null)
            {
                Debug.LogWarning("Move Initial Combatants failed. Active scene에서 WorldManager를 찾을 수 없습니다.");
                return;
            }

            PlayerController playerController = Object.FindAnyObjectByType<PlayerController>();
            MonsterController monsterController = Object.FindAnyObjectByType<MonsterController>();
            if (playerController == null || monsterController == null)
            {
                Debug.LogWarning("Move Initial Combatants failed. Active scene에서 Player 또는 Monster를 찾을 수 없습니다.");
                return;
            }

            Vector3Int playerCellPos = new Vector3Int(0, 0, 0);
            Vector3Int monsterCellPos = new Vector3Int(0, 2, 0);

            if (!worldManager.TryGetGroundWorldPos(playerCellPos, out Vector3 playerWorldPos) ||
                !worldManager.TryGetGroundWorldPos(monsterCellPos, out Vector3 monsterWorldPos))
            {
                Debug.LogWarning("Move Initial Combatants failed. 초기 셀에 Ground 타일이 없습니다.");
                return;
            }

            // WorldManager.Awake의 임시 전투 배치와 같은 위치로 되돌립니다.
            playerWorldPos.z = playerController.transform.position.z;
            monsterWorldPos.z = monsterController.transform.position.z;
            playerController.transform.position = playerWorldPos;
            monsterController.transform.position = monsterWorldPos;
            playerController.Model.SetCellPos(playerCellPos);
            monsterController.Model.SetCellPos(monsterCellPos);

            Debug.Log("Move Initial Combatants complete.");
        }
    }
}
