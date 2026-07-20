using MBT;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    [AddComponentMenu("")]
    [MBTNode(name = "Creature/Leaf - TakeJob")]
    public class TakeJob : Leaf
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
            if (CreatureJobPool.TryFind(out CreatureMiningJob miningJob) == false)
            {
                return NodeResult.failure;
            }

            // 아직 배정되지 않은 채굴 Job 하나를 현재 Creature의 실행 큐로 옮깁니다.
            CreatureJobPool.Remove(miningJob);
            miningJob.SetCreatureController(controller);
            controller.JobQueue.Enqueue(miningJob);

            return NodeResult.success;
        }

        private void CacheComponents()
        {
            controller = GetComponentInParent<CreatureController>();
        }
    }
}
