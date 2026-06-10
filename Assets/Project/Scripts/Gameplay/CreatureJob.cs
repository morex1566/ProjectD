using UnityEngine;

namespace TRPG.Runtime
{
    public abstract class CreatureJob
    {
        public int Priority;

        public bool IsDone;

        protected CreatureController owner;

        protected CreatureJobQueue queue;

        protected CreatureJob(CreatureController owner, CreatureJobQueue queue, int priority)
        {
            this.owner = owner;
            this.queue = queue;
            Priority = priority;
        }

        public virtual void Execute()
        {
            IsDone = EvaluteIsDone();
        }

        public abstract bool EvaluteIsDone();
    }
}
