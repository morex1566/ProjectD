using MBT;
using UnityEngine;

namespace TRPG.Runtime
{
    [AddComponentMenu("")]
    [MBTNode(name = "Creature/Condition - IsWanderJob")]
    public class IsWanderJob : Condition
    {
        [SerializeField, ReadOnly] private CreatureController controller = null;

        private void OnValidate()
        {
            CacheComponents();
        }

        private void Awake()
        {
            CacheComponents();
        }

        public override bool Check()
        {
            return controller.JobQueue.TryPeek(out CreatureJob job) == true && job is CreatureWanderJob;
        }

        private void CacheComponents()
        {
            controller = GetComponentInParent<CreatureController>();
        }
    }
}
