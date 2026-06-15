using UnityEngine;

namespace TRPG.Runtime
{
    public class TestRunner : MonoBehaviour
    {
        [SerializeField] private IdKeyData allyIdData;

        [ContextMenu("TestSpawnAlly")]
        public void TestSpawnAlly()
        {
            var worldPos = WorldManager.MapGenerator.Center;
            var creature = WorldManager.Spawn(allyIdData, (Vector2)worldPos);
            var creatureController = creature as CreatureController;
            creatureController?.SetOwner(PlayerManager.GetInstance().gameObject);
        }
    }
}
