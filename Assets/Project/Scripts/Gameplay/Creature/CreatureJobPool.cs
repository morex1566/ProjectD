using System.Collections.Generic;

namespace TRPG.Runtime
{
    /// <summary>
    /// 아직 Creature에게 배정되지 않은 작업을 보관합니다.
    /// </summary>
    public static class CreatureJobPool
    {
        private static readonly List<CreatureJob> jobs = new();

        public static IReadOnlyList<CreatureJob> Jobs => jobs;

        public static void Add(CreatureJob job)
        {
            if (job == null)
            {
                return;
            }

            jobs.Add(job);
        }

        public static bool Remove(CreatureJob job)
        {
            return jobs.Remove(job);
        }

        public static List<T> Find<T>() where T : CreatureJob
        {
            List<T> results = new();

            for (int i = 0; i < jobs.Count; i++)
            {
                // 요청한 타입의 CreatureJob만 수집합니다.
                if (jobs[i] is T typedJob)
                {
                    results.Add(typedJob);
                }
            }

            return results;
        }
    }
}
