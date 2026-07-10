using MBT;
using UnityEngine;

namespace TRPG.Runtime
{
    [AddComponentMenu("")]
    [MBTNode(name = "Creature/Leaf - TakeEngageJobAuto")]
    public class TakeEngageJobAuto : Leaf
    {
        [SerializeField, ReadOnly] private CreatureController controller = null;

        [SerializeField, ReadOnly] private CreatureDetector detector = null;

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
            if (CanTakeEngageJob() == false)
            {
                return NodeResult.failure;
            }

            if (TryFindEngageTarget(out CreatureController engageTarget) == false)
            {
                return NodeResult.failure;
            }

            controller.JobQueue.Enqueue(new CreatureEngageJob(controller, engageTarget));
            return NodeResult.success;
        }

        private bool CanTakeEngageJob()
        {
            if (controller.IsDead() == true)
            {
                return false;
            }

            if (controller.JobQueue.TryFind<CreatureEngageJob>(out _) == true)
            {
                return false;
            }

            if (controller.JobQueue.Count > 0 &&
                controller.JobQueue.TryPeek<CreatureWanderJob>(out _) == false)
            {
                return false;
            }

            // 빈 큐이거나 방랑 중일 때만 자동 전투가 기존 행동을 가로챕니다.
            return true;
        }

        private bool TryFindEngageTarget(out CreatureController engageTarget)
        {
            engageTarget = null;

            foreach (CreatureController detected in detector.Detecteds)
            {
                if (Faction.IsHostile(controller.Context.Faction, detected.Context.Faction) == true)
                {
                    engageTarget = detected;
                    return true;
                }
            }

            return false;
        }

        private void CacheComponents()
        {
            controller = gameObject.GetComponentInHierarchy<CreatureController>();
            detector = gameObject.GetComponentInHierarchy<CreatureDetector>();
        }
    }
}
