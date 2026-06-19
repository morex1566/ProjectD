using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 에디터 ContextMenu에서 런타임 동작을 빠르게 확인하기 위한 테스트 컴포넌트입니다.
    /// </summary>
    public class TestRunner : MonoBehaviour
    {
        [SerializeField] private IdKeyData allyIdData;

        /// <summary>
        /// 설정된 IdKeyData를 사용해 플레이어 소유 Creature를 시작 위치에 생성합니다.
        /// </summary>
        [ContextMenu("TestSpawnAlly")]
        public void TestSpawnAlly()
        {
            var creature = WorldManager.Spawn(allyIdData, PlayerManager.GetInstance().gameObject, WorldManager.MapController.MapData.StartSpawnPoint);
            var creatureController = creature as CreatureController;
        }
    }
}
