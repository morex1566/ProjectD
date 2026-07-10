using MBT;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    [AddComponentMenu("")]
    [MBTNode(name = "Creature/Leaf - TakeJobFromPool")]
    public class TakeJobFromPool : Leaf
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
            if (TryTakeMiningJob() == true)
            {
                return NodeResult.success;
            }

            return NodeResult.failure;
        }

        private void CacheComponents()
        {
            controller = GetComponentInParent<CreatureController>();
        }

        private bool TryTakeMiningJob()
        {
            List<CreatureMiningJob> miningJobs = CreatureJobPool.Find<CreatureMiningJob>();
            if (miningJobs.Count <= 0)
            {
                return false;
            }

            // 아직 배정되지 않은 채굴 Job 하나를 현재 Creature의 실행 큐로 옮깁니다.
            CreatureMiningJob miningJob = miningJobs[0];
            miningJob.SetCreatureController(controller);
            CreatureJobPool.Remove(miningJob);
            controller.JobQueue.Enqueue(miningJob);

            return true;
        }
    }
}
