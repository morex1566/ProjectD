using Codice.Client.Common.GameUI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    public readonly struct JobEntry
    {
        ///<summary>
        /// 실행할 CreatureJob입니다.
        ///</summary>
        public readonly CreatureJob Job;

        ///<summary>
        /// Queue에 추가된 순서입니다.
        ///</summary>
        public readonly int Sequence;

        ///<summary>
        /// JobEntry를 생성합니다.
        ///</summary>
        public JobEntry(CreatureJob job, int sequence)
        {
            Job = job;
            Sequence = sequence;
        }
    }

    /// <summary>
    /// Creature가 순서대로 실행할 Job 큐를 관리합니다.
    /// </summary>
    public class CreatureJobQueue 
    {
        private CreatureController controller = null;

        ///<summary>
        /// 같은 Priority를 가진 Job의 입력 순서를 보존하기 위한 번호입니다.
        ///</summary>
        private int sequence = 0;

        ///<summary>
        /// Job 목록 정렬이 필요한지 여부입니다.
        ///</summary>
        private bool isDirty = false;

        private readonly List<JobEntry> jobs = new();


        public CreatureJobQueue(CreatureController controller)
        {
            this.controller = controller;
        }


        /// <summary>
        /// 새 CreatureJob을 실행 대기 큐 끝에 추가합니다.
        /// </summary>
        public void Enqueue(CreatureJob job)
        {

            jobs.Add(new JobEntry(job, sequence));
            sequence++;
            isDirty = true;
        }

        /// <summary>
        /// 큐 앞쪽의 CreatureJob을 꺼냅니다.
        /// </summary>
        public bool TryDequeue(out CreatureJob job)
        {
            if (jobs.Count <= 0)
            {
                job = null;
                return false;
            }

            job = jobs[0].Job;
            jobs.RemoveAt(0);
            return true;
        }

        public bool TryFind<T>(out T job) where T : CreatureJob
        {
            foreach (JobEntry entry in jobs)
            {
                // 원하는 타입의 Job이면 반환합니다.
                if (entry.Job is T typedJob)
                {
                    job = typedJob;
                    return true;
                }
            }

            job = null;
            return false;
        }

        ///<summary>
        /// 특정 CreatureJob을 제거합니다.
        ///</summary>
        public bool Remove(CreatureJob targetJob)
        {
            for (int i = 0; i < jobs.Count; i++)
            {
                // 같은 참조의 Job을 찾으면 제거합니다.
                if (jobs[i].Job == targetJob)
                {
                    jobs.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        ///<summary>
        /// 조건에 맞는 CreatureJob을 모두 제거합니다.
        ///</summary>
        public int RemoveWhere(Predicate<CreatureJob> predicate)
        {
            int removedCount = 0;

            for (int i = jobs.Count - 1; i >= 0; i--)
            {
                // 조건에 맞는 Job이면 제거합니다.
                if (predicate(jobs[i].Job))
                {
                    jobs.RemoveAt(i);
                    removedCount++;
                }
            }

            return removedCount;
        }

        ///<summary>
        /// 대기 중인 모든 CreatureJob을 제거합니다.
        ///</summary>
        public void Clear()
        {
            jobs.Clear();
            isDirty = false;
        }

        ///<summary>
        /// 현재 맨 앞 Job 하나만 실행합니다.
        ///</summary>
        public void Update()
        {
            Sort();

            while (jobs.Count > 0)
            {
                CreatureJob job = jobs[0].Job;

                if (job.IsDone)
                {
                    jobs.RemoveAt(0);
                    continue;
                }

                job.Evaluate();
                return;
            }
        }

        ///<summary>
        /// Job의 기즈모를 그립니다.
        ///</summary>
        public void DrawGizmos()
        {
            foreach (JobEntry entry in jobs)
            {
                entry.Job.DrawGizmos();
            }
        }

        ///<summary>
        /// 필요할 때만 Job 목록을 정렬합니다.
        ///</summary>
        private void Sort()
        {
            if (!isDirty)
            {
                return;
            }

            jobs.Sort(CompareJobEntry);
            isDirty = false;
        }

        ///<summary>
        /// JobEntry의 실행 순서를 비교합니다.
        ///</summary>
        private int CompareJobEntry(JobEntry a, JobEntry b)
        {
            int priorityCompare = a.Job.Priority.CompareTo(b.Job.Priority);

            if (priorityCompare != 0)
            {
                return priorityCompare;
            }

            return a.Sequence.CompareTo(b.Sequence);
        }

    }
}
