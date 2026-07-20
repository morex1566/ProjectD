using MBT;
using UnityEngine;

namespace TRPG.Runtime
{
    [AddComponentMenu("")]
    [MBTNode(name = "Creature/Leaf - DoEngage")]
    public class DoEngage : Leaf
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

        public override NodeResult Execute()
        {
            if (controller == null || controller.IsDead() == true)
            {
                return NodeResult.failure;
            }

            if (controller.JobQueue.TryPeek(out CreatureEngageJob job) == false)
            {
                return NodeResult.failure;
            }

            return NodeResult.running;
        }

        private void CacheComponents()
        {
            controller = gameObject.GetComponentInHierarchy<CreatureController>();
        }
    }
}
