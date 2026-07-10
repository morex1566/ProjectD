using MBT;
using UnityEngine;

namespace TRPG.Runtime
{
    [AddComponentMenu("")]
    [MBTNode(name = "Creature/Leaf - DoMining")]
    public class DoMining : Leaf
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
            if (controller == null || controller.IsDead() == true || controller.Context.MoveSpeed <= 0f)
            {
                return NodeResult.failure;
            }

            if (controller.JobQueue.TryPeek(out CreatureJob job) == false || job is not CreatureMiningJob miningJob)
            {
                return NodeResult.failure;
            }

            if (controller.TryEnsureMiningPath(miningJob) == false)
            {
                return NodeResult.failure;
            }

            if (controller.MoveAlongMiningJob(miningJob) == true)
            {
                // 실제 채굴 처리 로직이 붙기 전까지는 도착 상태로 Job을 유지합니다.
                return NodeResult.running;
            }

            return NodeResult.running;
        }

        private void CacheComponents()
        {
            controller = GetComponentInParent<CreatureController>();
        }
    }
}
