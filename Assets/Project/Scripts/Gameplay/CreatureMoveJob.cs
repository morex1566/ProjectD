using UnityEngine;

namespace TRPG.Runtime
{
    public class CreatureMoveJob : CreatureJob
    {
        public static float distanceThreshold = 0.1f;

        private Vector3 targetPos = Vector3.zero;

        private Vector3 direction;



        public CreatureMoveJob(Vector3 targetPos, float moveSpeed, CreatureController owner, CreatureJobMachine queue, int priority) : base(owner, queue, priority)
        {
            this.targetPos = targetPos;
        }



        public override void Execute()
        {
            base.Execute();

            MoveTo();
            SetAnim();
        }

        public override bool EvaluteIsDone()
        {
            if (owner == null) return true;

            float sqrDistance = (owner.transform.position - targetPos).sqrMagnitude;

            return sqrDistance <= distanceThreshold * distanceThreshold;
        }

        private void MoveTo()
        {
            Vector3 currentPos = owner.transform.position;
            Vector3 nextPos = Vector3.MoveTowards(currentPos, targetPos, owner.Status.MoveSpeed * Time.deltaTime);
  
            direction = nextPos - currentPos;
            owner.transform.position = nextPos;
        }

        private void SetAnim()
        {
            // 목적지에 도착
            if (EvaluteIsDone())
            {
                owner.Spriter.flipX = false;
                Vector3 euler = owner.transform.rotation.eulerAngles;
                euler.y = 0f;
                owner.transform.rotation = Quaternion.Euler(euler);

                return;
            }

            // 캐릭터가 오른쪽으로 가는중?
            if (direction.x > 0f)
            {
                owner.Spriter.flipX = false;
                Vector3 euler = owner.transform.rotation.eulerAngles;
                euler.y = -18f;
                owner.transform.rotation = Quaternion.Euler(euler);
            }
            // 캐릭터가 왼쪽으로 가는중?
            else
            {
                owner.Spriter.flipX = true;
                Vector3 euler = owner.transform.rotation.eulerAngles;
                euler.y = 18f;
                owner.transform.rotation = Quaternion.Euler(euler);
            }
        }
    }
}
