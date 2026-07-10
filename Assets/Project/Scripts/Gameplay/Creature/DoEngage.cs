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

            if (controller.JobQueue.TryPeek(out CreatureJob job) == false || job is not CreatureEngageJob engageJob)
            {
                return NodeResult.failure;
            }

            if (controller.IsEngageTargetValid(engageJob) == false)
            {
                engageJob.Complete();
                return NodeResult.success;
            }

            if (controller.IsInAttackRange(engageJob) == true)
            {
                engageJob.ClearPath();
                return NodeResult.running;
            }

            if (controller.TryEnsureEngagePath(engageJob) == false)
            {
                return NodeResult.failure;
            }

            controller.MoveAlongEngageJob(engageJob);
            return NodeResult.running;
        }

        private void CacheComponents()
        {
            controller = gameObject.GetComponentInHierarchy<CreatureController>();
        }
    }
}
