using System;
using System.Collections.Generic;

namespace TRPG.Runtime
{
    /// <summary>
    /// 같은 우선순위의 Job 입력 순서를 보존하기 위한 큐 항목입니다.
    /// </summary>
    public readonly struct CreatureJobEntry
    {
        public CreatureJob Job { get; }

        public int Sequence { get; }


        public CreatureJobEntry(CreatureJob job, int sequence)
        {
            Job = job;
            Sequence = sequence;
        }
    }

    /// <summary>
    /// Creature가 실행할 Job을 우선순위와 입력 순서에 따라 관리합니다.
    /// </summary>
    public class CreatureJobQueue
    {
        private readonly List<CreatureJobEntry> jobs = new();

        private int sequence = 0;

        private bool isDirty = false;


        public int Count => jobs.Count;


        /// <summary>
        /// 새 CreatureJob을 실행 대기 큐에 추가합니다.
        /// </summary>
        public void Enqueue(CreatureJob job)
        {
            if (job == null)
            {
                return;
            }

            job.Completed += HandleJobCompleted;
            jobs.Add(new CreatureJobEntry(job, sequence));
            sequence++;
            isDirty = true;
        }

        /// <summary>
        /// 큐 앞쪽의 CreatureJob을 제거하지 않고 확인합니다.
        /// </summary>
        public bool TryPeek(out CreatureJob job)
        {
            Sort();

            if (jobs.Count <= 0)
            {
                job = null;
                return false;
            }

            job = jobs[0].Job;
            return true;
        }

        /// <summary>
        /// 큐 앞쪽 Job이 지정한 타입일 때 반환합니다.
        /// </summary>
        public bool TryPeek<T>(out T job) where T : CreatureJob
        {
            if (TryPeek(out CreatureJob currentJob) == false || currentJob is T typedJob == false)
            {
                job = null;
                return false;
            }

            job = typedJob;
            return true;
        }

        /// <summary>
        /// 큐 앞쪽의 CreatureJob을 꺼냅니다.
        /// </summary>
        public bool TryDequeue(out CreatureJob job)
        {
            Sort();

            if (jobs.Count <= 0)
            {
                job = null;
                return false;
            }

            job = jobs[0].Job;
            RemoveAt(0);
            return true;
        }

        /// <summary>
        /// 큐에 대기 중인 지정 타입의 첫 Job을 찾습니다.
        /// </summary>
        public bool TryFind<T>(out T job) where T : CreatureJob
        {
            foreach (CreatureJobEntry entry in jobs)
            {
                if (entry.Job is T typedJob)
                {
                    job = typedJob;
                    return true;
                }
            }

            job = null;
            return false;
        }

        /// <summary>
        /// 같은 참조의 CreatureJob을 제거합니다.
        /// </summary>
        public bool Remove(CreatureJob targetJob)
        {
            for (int index = 0; index < jobs.Count; index++)
            {
                if (jobs[index].Job == targetJob)
                {
                    RemoveAt(index);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 조건에 맞는 CreatureJob을 모두 제거합니다.
        /// </summary>
        public int RemoveWhere(Predicate<CreatureJob> predicate)
        {
            if (predicate == null)
            {
                return 0;
            }

            int removedCount = 0;

            for (int index = jobs.Count - 1; index >= 0; index--)
            {
                if (predicate(jobs[index].Job) == false)
                {
                    continue;
                }

                RemoveAt(index);
                removedCount++;
            }

            return removedCount;
        }

        /// <summary>
        /// 대기 및 실행 중인 모든 CreatureJob을 제거합니다.
        /// </summary>
        public void Clear()
        {
            for (int index = 0; index < jobs.Count; index++)
            {
                jobs[index].Job.Completed -= HandleJobCompleted;
            }

            jobs.Clear();
            isDirty = false;
        }

        private void Sort()
        {
            if (isDirty == false)
            {
                return;
            }

            jobs.Sort(Compare);
            isDirty = false;
        }

        private static int Compare(CreatureJobEntry left, CreatureJobEntry right)
        {
            int priorityComparison = right.Job.PriorityInJobQueue.CompareTo(left.Job.PriorityInJobQueue);

            if (priorityComparison != 0)
            {
                return priorityComparison;
            }

            return left.Sequence.CompareTo(right.Sequence);
        }

        private void HandleJobCompleted(CreatureJob job)
        {
            Remove(job);
        }

        private void RemoveAt(int index)
        {
            jobs[index].Job.Completed -= HandleJobCompleted;
            jobs.RemoveAt(index);
        }
    }
}
