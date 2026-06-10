using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    public class CreatureJobQueue 
    {
        private readonly Queue<CreatureJob> jobs = new();

        public void Enqueue(CreatureJob job)
        {
            jobs.Enqueue(job);
        }

        public bool TryDequeue(out CreatureJob job)
        {
            return jobs.TryDequeue(out job);
        }

        /// <summary>
        /// 현재 맨 앞 Job 하나만 실행한다.
        /// 매 Update에서 호출하면 된다.
        /// </summary>
        public void Execute()
        {
            while (jobs.TryPeek(out CreatureJob job))
            {
                if (job.IsDone)
                {
                    jobs.Dequeue();
                    continue;
                }

                job.Execute();

                if (job.IsDone)
                {
                    jobs.Dequeue();
                }

                return;
            }
        }
    }
}
