using MBT;
using UnityEngine;

namespace TRPG.Runtime
{
    [AddComponentMenu("")]
    [MBTNode(name = "Creature/Leaf - TakeWanderJobAuto")]
    public class TakeWanderJobAuto : Leaf
    {
        [SerializeField, ReadOnly] private CreatureController controller = null;

        [SerializeField] private int wanderRadius = 4;

        [SerializeField] private IntRange startDelaySec;

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
            if (CanTakeWanderJob() == false)
            {
                return NodeResult.failure;
            }

            controller.JobQueue.Enqueue(new CreatureWanderJob(controller, wanderRadius, startDelaySec.Random()));
            return NodeResult.success;
        }

        private bool CanTakeWanderJob()
        {
            if (controller.IsDead() == true)
            {
                return false;
            }

            if (controller.JobQueue.Count > 0)
            {
                return false;
            }

            if (controller.Context.MoveSpeed <= 0f)
            {
                return false;
            }

            return true;
        }

        private void CacheComponents()
        {
            controller = gameObject.GetComponentInHierarchy<CreatureController>();
        }
    }
}
