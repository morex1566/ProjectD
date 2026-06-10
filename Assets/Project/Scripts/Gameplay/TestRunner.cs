using UnityEngine;

namespace TRPG.Runtime
{
    public class TestRunner : MonoBehaviour
    {
        [SerializeField] private IdKeyData idKeyData;

        [ContextMenu("TestSpawn")]
        public void TestSpawn()
        {
            WorldManager.Spawn(idKeyData, Vector2.zero);
        }
    }
}
