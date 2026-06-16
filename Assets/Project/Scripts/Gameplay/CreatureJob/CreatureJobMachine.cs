using Codice.Client.Common.GameUI;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    public class CreatureJobMachine 
    {
        private readonly Queue<CreatureJob> queue = new();

        public void Enqueue(CreatureJob job)
        {
            queue.Enqueue(job);
        }

        public bool TryDequeue(out CreatureJob job)
        {
            return queue.TryDequeue(out job);
        }

        public void Clear()
        {
            queue.Clear();
        }

        /// <summary>
        /// 현재 맨 앞 Job 하나만 실행한다.
        /// 매 Update에서 호출하면 된다.
        /// </summary>
        public void Execute()
        {
            while (queue.TryPeek(out CreatureJob job))
            {
                if (job.IsDone)
                {
                    queue.Dequeue();
                    continue;
                }

                job.Execute();

                if (job.IsDone)
                {
                    queue.Dequeue();
                }

                return;
            }
        }
    }
}
