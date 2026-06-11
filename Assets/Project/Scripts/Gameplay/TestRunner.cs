using UnityEngine;

namespace TRPG.Runtime
{
    public class TestRunner : MonoBehaviour
    {
        [SerializeField] private IdKeyData allyIdData;

        [SerializeField] private IdKeyData enemyIdData;

        [ContextMenu("TestSpawnAlly")]
        public void TestSpawnAlly()
        {
            var creature = WorldManager.Spawn(allyIdData, Vector2.left);
            var creatureController = creature as CreatureController;
            creatureController?.SetOwner(PlayerManager.GetInstance().gameObject);
        }

        [ContextMenu("TestSpawnEnemy")]
        public void TestSpawnEnemy()
        {
            var creature = WorldManager.Spawn(enemyIdData, Vector2.right);
        }
    }
}
