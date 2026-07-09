using MBT;
using UnityEngine;

namespace TRPG.Runtime
{
    [AddComponentMenu("")]
    [MBTNode(name = "Creature/Condition - CanMove")]
    public class CanMove : Condition
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

            return controller.JobQueue.TryPeek(out CreatureJob job) == true && job is CreatureMoveJob;
        }

        private void CacheComponents()
        {
            controller = GetComponentInParent<CreatureController>();
        }
    }
}
