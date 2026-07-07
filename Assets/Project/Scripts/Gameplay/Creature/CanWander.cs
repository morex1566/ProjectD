using MBT;
using UnityEngine;

namespace TRPG.Runtime
{
    [AddComponentMenu("")]
    [MBTNode(name = "Creature/Condition - CanWander")]
    public class CanWander : Condition
    {
        [SerializeField, ReadOnly] private CreatureController controller = null;

        private void Awake()
        {
            controller = GetComponentInParent<CreatureController>();
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

            return true;
        }
    }
}
