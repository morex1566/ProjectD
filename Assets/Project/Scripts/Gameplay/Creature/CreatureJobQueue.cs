using Codice.Client.Common.GameUI;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// Creature가 순서대로 실행할 Job 큐를 관리합니다.
    /// </summary>
    public class CreatureJobQueue 
    {
        private CreatureController owner = null;

        private readonly Queue<CreatureJob> queue = new();


        public CreatureJobQueue(CreatureController owner)
        {
            this.owner = owner;
        }


        /// <summary>
        /// 새 CreatureJob을 실행 대기 큐 끝에 추가합니다.
        /// </summary>
        public void Enqueue(CreatureJob job)
        {
            queue.Enqueue(job);
        }

        /// <summary>
        /// 큐 앞쪽의 CreatureJob을 꺼냅니다.
        /// </summary>
        public bool TryDequeue(out CreatureJob job)
        {
            return queue.TryDequeue(out job);
        }

        /// <summary>
        /// 대기 중인 모든 CreatureJob을 제거합니다.
        /// </summary>
        public void Clear()
        {
            queue.Clear();
        }

        /// <summary>
        /// 현재 맨 앞 Job 하나만 실행한다.
        /// 매 Update에서 호출하면 된다.
        /// </summary>
        public void Update()
        {
            while (queue.TryPeek(out CreatureJob job))
            {
                if (job.IsDone)
                {
                    queue.Dequeue();
                    continue;
                }

                job.Evaluate();

                return;
            }
        }

        /// <summary>
        /// Job의 드로우 기즈모
        /// </summary>
        public void DrawGizmos()
        {
            foreach (var job in queue)
            {
                job.DrawGizmos();
            }
        }
    }
}
