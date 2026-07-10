using MBT;
using System;
using UnityEngine;

namespace TRPG.Runtime
{
    [AddComponentMenu("")]
    [MBTNode(name = "Creature/Leaf - TakeJobAuto")]
    public class TakeJobAuto : Leaf
    {
        [SerializeField, ReadOnly] private CreatureController controller = null;

        [SerializeField, ReadOnly] private CreatureDetector detector = null;

        [Header(nameof(CreatureWanderJob))]

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
            // 이미 전투 Job이 있으면 같은 적을 중복으로 큐에 넣지 않습니다.
            if (CanTakeAutoJob() == false)
            {
                return NodeResult.failure;
            }

            // 적이 감지되었음.
            if (TryTakeEngageJob(out CreatureController engageTarget) == true)
            {
                controller.JobQueue.Enqueue(new CreatureEngageJob(controller, engageTarget));
                return NodeResult.success;
            }

            // 할게 없으면 방랑
            if (TryTakeWanderJob() == true)
            {
                controller.JobQueue.Enqueue(new CreatureWanderJob(controller, wanderRadius, startDelaySec.Random()));
                return NodeResult.success;
            }

            // 이도 저도 아님
            return NodeResult.failure;
        }

        private bool CanTakeAutoJob()
        {
            if (controller.JobQueue.TryFind<CreatureEngageJob>(out _) == true)
            {
                return false;
            }

            if (controller.IsDead() == true)
            {
                return false;
            }

            return true;
        }

        private bool TryTakeEngageJob(out CreatureController engageTarget)
        {
            engageTarget = default;

            // 뭔가 하고 있다는 뜻
            if (controller.JobQueue.Count > 0 &&
                controller.JobQueue.TryPeek<CreatureWanderJob>(out _) == false)
            {
                return false;
            }

            // 대상 찾기
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

        private bool TryTakeWanderJob()
        {
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
            detector = gameObject.GetComponentInHierarchy<CreatureDetector>();
        }
    }
}
