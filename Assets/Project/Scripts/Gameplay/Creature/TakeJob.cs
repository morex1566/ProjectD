using MBT;
using UnityEngine;

namespace TRPG.Runtime
{
    [AddComponentMenu("")]
    [MBTNode(name = "Creature/Leaf - TakeJob")]
    public class TakeJob : Leaf
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
            if (controller == null || controller.IsDead() == true)
            {
                return NodeResult.failure;
            }

            // 전투는 빈 큐 또는 Wander를 실행 중일 때 우선 배정합니다.
            if (TryTakeEngageJob() == true)
            {
                return NodeResult.success;
            }

            // 외부에서 생성된 공용 작업을 먼저 가져옵니다.
            if (TryTakeMiningJob() == true)
            {
                return NodeResult.success;
            }

            // 전투가 아닌 Job은 Creature가 대기 중일 때만 배정합니다.
            if (controller.JobQueue.Count > 0)
            {
                return NodeResult.failure;
            }

            // 가져올 작업이 없으면 Wander를 생성합니다.
            if (TryTakeWanderJob() == true)
            {
                return NodeResult.success;
            }

            return NodeResult.failure;
        }

        /// <summary>
        /// 감지된 적을 대상으로 EngageJob을 생성합니다.
        /// 기존 WanderJob은 완료하고 전투로 교체합니다.
        /// </summary>
        private bool TryTakeEngageJob()
        {
            if (detector == null)
            {
                return false;
            }

            if (controller.JobQueue.TryFind<CreatureEngageJob>(out _) == true)
            {
                return false;
            }

            bool isIdle = controller.JobQueue.Count == 0;
            bool isWandering = controller.JobQueue.TryPeek(out CreatureWanderJob wanderJob);

            if (isIdle == false || isWandering == false)
            {
                return false;
            }

            if (TryFindEngageTarget(out CreatureController target) == false)
            {
                return false;
            }

            if (isWandering == true)
            {
                wanderJob.Complete();
            }

            controller.JobQueue.Enqueue(new CreatureEngageJob(controller, target));
            return true;
        }

        /// <summary>
        /// 공용 JobPool에서 아직 배정되지 않은 MiningJob을 가져옵니다.
        /// </summary>
        private bool TryTakeMiningJob()
        {
            if (CreatureJobPool.TryFind(out CreatureMiningJob miningJob) == false)
            {
                return false;
            }

            if (CreatureJobPool.Remove(miningJob) == false)
            {
                return false;
            }

            miningJob.SetCreatureController(controller);
            controller.JobQueue.Enqueue(miningJob);

            return true;
        }

        /// <summary>
        /// 다른 작업이 없을 때 WanderJob을 생성합니다.
        /// </summary>
        private bool TryTakeWanderJob()
        {
            if (controller.Context.MoveSpeed <= 0f)
            {
                return false;
            }

            controller.JobQueue.Enqueue(new CreatureWanderJob(controller));
            return true;
        }

        /// <summary>
        /// 감지된 Creature 중 현재 Creature와 적대 관계인 유효한 대상을 반환합니다.
        /// </summary>
        private bool TryFindEngageTarget(out CreatureController engageTarget)
        {
            engageTarget = null;

            foreach (CreatureController detected in detector.Detecteds)
            {
                if (detected == null || detected == controller || detected.isActiveAndEnabled == false || detected.IsDead() == true)
                {
                    continue;
                }

                if (Faction.IsHostile(controller.Context.Faction, detected.Context.Faction) == false)
                {
                    continue;
                }

                engageTarget = detected;
                return true;
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