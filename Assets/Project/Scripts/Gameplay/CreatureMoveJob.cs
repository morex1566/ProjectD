using UnityEngine;

namespace TRPG.Runtime
{
    public class CreatureMoveJob : CreatureJob
    {
        public static float distanceThreshold = 0.1f;

        private Vector3 targetPos = Vector3.zero;

        private float moveSpeed;

        public CreatureMoveJob(Vector3 targetPos, float moveSpeed, CreatureController owner, CreatureJobQueue queue, int priority) : base(owner, queue, priority)
        {
            this.targetPos = targetPos;
            this.moveSpeed = moveSpeed;
        }

        public override void Execute()
        {
            base.Execute();

            MoveTo();
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

            owner.transform.position = Vector3.MoveTowards(currentPos, targetPos, moveSpeed * Time.deltaTime);
        }
    }
}
