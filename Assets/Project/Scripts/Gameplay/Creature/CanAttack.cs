using MBT;
using UnityEngine;

namespace TRPG.Runtime
{
    [AddComponentMenu("")]
    [MBTNode(name = "Creature/Condition - CanAttack")]
    public class CanAttack : Condition
    {
        [SerializeField, ReadOnly] private CreatureController controller = null;

        public override void OnEnter()
        {
            base.OnEnter();

            controller = GetComponentInParent<CreatureController>();
        }

        public override bool Check()
        {
            return controller.Context.State.HasFlag(CreatureStateType.Dead) == false;
        }
    }
}
