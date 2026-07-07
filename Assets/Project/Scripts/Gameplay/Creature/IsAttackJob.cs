using MBT;
using UnityEngine;

namespace TRPG.Runtime
{
    [AddComponentMenu("")]
    [MBTNode(name = "Creature/Condition - IsAttackJob")]
    public class IsAttackJob : Condition
    {
        private const string AttackJobTypeName = "CreatureAttackJob";

        [SerializeField, ReadOnly] private CreatureController controller = null;

        public override void OnEnter()
        {
            controller = GetComponentInParent<CreatureController>();

            base.OnEnter();
        }

        public override bool Check()
        {
            return controller.JobQueue.TryPeek(out CreatureJob job) == true && job.GetType().Name == AttackJobTypeName;
        }
    }
}
