using MBT;
using UnityEngine;

namespace TRPG.Runtime
{
    [AddComponentMenu("")]
    [MBTNode(name = "Creature/Condition - CanMining")]
    public class CanMining : Condition
    {
        [SerializeField, ReadOnly] private CreatureController controller = null;

        public override void OnEnter()
        {
            controller = GetComponentInParent<CreatureController>();

            base.OnEnter();
        }

        public override bool Check()
        {
            return controller.IsDead() == false;
        }
    }
}
