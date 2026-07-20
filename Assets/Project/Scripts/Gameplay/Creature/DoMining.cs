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

            if (controller.JobQueue.TryPeek(out CreatureMiningJob job) == false)
            {
                return NodeResult.failure;
            }

            return NodeResult.running;
        }

        private void CacheComponents()
        {
            controller = GetComponentInParent<CreatureController>();
        }
    }
}
