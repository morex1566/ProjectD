using MBT;
using UnityEngine;

namespace TRPG.Runtime
{
    [AddComponentMenu("")]
    [MBTNode(name = "Creature/Condition - CanMove")]
    public class CanMove : Condition
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

            return controller.Context.MoveSpeed > 0f;
        }
    }
}
