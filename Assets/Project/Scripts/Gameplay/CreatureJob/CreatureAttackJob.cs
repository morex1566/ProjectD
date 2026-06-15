using UnityEngine;

namespace TRPG.Runtime
{
    public class CreatureAttackJob : CreatureJob
    {
        public static float MaxAttackGauge = 100f;

        private CreatureController target;

        private Vector3 direction;

        // 100을 넘으면 공격할 수 있습니다!
        private float attackGauge = 90f;

        public CreatureAttackJob(CreatureController target, CreatureController owner, CreatureJobMachine queue, int priority) : base(owner, queue, priority)
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
        }

        private void Attack()
        {
            // 일단 공격 가능한 범위까지 이동
            if (!owner.Detector.Detect(target))
            {
                Vector3 currentPos = owner.transform.position;
                Vector3 nextPos = Vector3.MoveTowards(currentPos, target.transform.position, owner.Status.MoveSpeed * Time.deltaTime);

                direction = nextPos - currentPos;
                owner.transform.position = nextPos;

                return;
            }

            // 공격 게이지를 채움
            if (attackGauge <= MaxAttackGauge)
            {
                attackGauge += owner.Status.AttackSpeed * Time.deltaTime * 100f;
            }
            // 공격 게이지가 꽉 차면 공격
            else
            {
                attackGauge = 0f;
                // TODO : 공격
            }
        }
    }
}
