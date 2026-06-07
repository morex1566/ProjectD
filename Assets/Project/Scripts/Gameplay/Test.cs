using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TRPG.Runtime
{
    public class Test : MonoBehaviour
    {
        [SerializeField] private Event testEvt;


        [ContextMenu("Test Event 실행")]
        public void TestEvent()
        {
            EventManager.Trigger<Event>(testEvt);
        }
    }
}
