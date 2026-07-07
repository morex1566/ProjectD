using MBT;
using UnityEngine;

namespace TRPG.Runtime
{
    [AddComponentMenu("")]
    [MBTNode(name = "Creature/Condition - CanWander")]
    public class CanWander : Condition
    {
        [SerializeField, ReadOnly] private CreatureController controller = null;

        public override void OnEnter()
        {
            // BT 노드는 Creature 하위에 있으므로 부모에서 런타임 컨트롤러를 찾습니다.
            controller = GetComponentInParent<CreatureController>();

            base.OnEnter();
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
