using MBT;
using UnityEngine;

namespace TRPG.Runtime
{
    [AddComponentMenu("")]
    [MBTNode(name = "Creature/Condition - CanMining")]
    public class CanMining : Condition
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

        public override bool Check()
        {
            if (controller.IsDead() == true)
            {
                return false;
            }

            if (controller.Context.MoveSpeed <= 0f)
            {
                return false;
            }

            return controller.JobQueue.TryPeek<CreatureMiningJob>(out _) == true;
        }

        private void CacheComponents()
        {
            controller = GetComponentInParent<CreatureController>();
        }
    }
}
