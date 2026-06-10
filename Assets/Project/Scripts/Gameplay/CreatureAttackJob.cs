using UnityEngine;

namespace TRPG.Runtime
{
    public class CreatureAttackJob : CreatureJob
    {
        private CreatureController target;

        public CreatureAttackJob(CreatureController target, CreatureController owner, CreatureJobQueue queue, int priority) : base(owner, queue, priority)
        {
            this.target = target;
        }

        public override bool EvaluteIsDone()
        {
            return target == null;
        }

        public override void Execute()
        {
            base.Execute();

            Attack();

            IsDone = true;
        }

        private void Attack()
        {
            Debug.Log($"{owner.name} attacked {target.name}");

            // TODO: 나중에 CreatureController에 체력/데미지 붙으면 여기서 처리
            // target.TakeDamage(owner.AttackPower);
        }
    }
}
