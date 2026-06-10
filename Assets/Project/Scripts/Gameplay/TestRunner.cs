using UnityEngine;

namespace TRPG.Runtime
{
    public class TestRunner : MonoBehaviour
    {
        [SerializeField] private IdKeyData idKeyData;

        [ContextMenu("TestSpawnAlly")]
        public void TestSpawnAlly()
        {
            var creature = WorldManager.Spawn(idKeyData, Vector2.zero);
            var creatureController = creature as CreatureController;
            creatureController?.SetOwner(PlayerManager.GetInstance().gameObject);
        }
    }
}
