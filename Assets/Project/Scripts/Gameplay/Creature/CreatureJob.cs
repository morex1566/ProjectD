using System;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// Creature 작업을 타입별로 등록 순서에 따라 관리합니다.
    /// </summary>
    public class CreatureJobPool
    {
        /// <summary>
        /// 등록된 Creature 작업 목록입니다.
        /// </summary>
        private readonly static List<CreatureJob> jobs = new();


        /// <summary>
        /// 등록된 작업의 개수입니다.
        /// </summary>
        public static int Count => jobs.Count;


        /// <summary>
        /// 작업을 등록합니다.
        /// </summary>
        public static void Add(CreatureJob job)
        {
            if (job == null)
            {
                return;
            }

            jobs.Add(job);
        }

        /// <summary>
        /// 지정된 타입 중 가장 먼저 등록된 작업을 반환합니다.
        /// </summary>
        public static bool TryFind<T>(out T job) where T : CreatureJob
        {
            for (int i = 0; i < jobs.Count; i++)
            {
                if (jobs[i] is not T targetJob)
                {
                    continue;
                }

                job = targetJob;
                return true;
            }

            job = null;
            return false;
        }

        /// <summary>
        /// 지정된 작업을 제거합니다.
        /// </summary>
        public static bool Remove(CreatureJob job)
        {
            return jobs.Remove(job);
        }

        /// <summary>
        /// 지정된 타입의 작업이 존재하는지 확인합니다.
        /// </summary>
        public static bool Contains<T>() where T : CreatureJob
        {
            for (int i = 0; i < jobs.Count; i++)
            {
                if (jobs[i] is T)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 모든 작업을 제거합니다.
        /// </summary>
        public static void Clear()
        {
            jobs.Clear();
        }
    }

    /// <summary>
    /// Creature가 순차 실행하는 작업 단위의 공통 베이스입니다.
    /// </summary>
    public abstract class CreatureJob
    {
        protected CreatureController controller = null;

        public virtual int PriorityInJobQueue => 0;

        public bool IsDone { get; private set; } = false;

        public event Action<CreatureJob> Completed;


        protected CreatureJob(CreatureController controller)
        {
            this.controller = controller;
        }

        protected CreatureJob()
        {

        }

        public void SetCreatureController(CreatureController controller)
        {
            this.controller = controller;
        }

        /// <summary>
        /// 작업을 완료하고 큐가 즉시 제거할 수 있도록 알립니다.
        /// </summary>
        public void Complete()
        {
            if (IsDone == true)
            {
                return;
            }

            IsDone = true;
            Completed?.Invoke(this);
        }
    }

    /// <summary>
    /// 현재 월드 길찾기 결과를 따라 Creature를 지정 타일 좌표로 이동시킵니다.
    /// </summary>
    public class CreatureMoveJob : CreatureJob
    {
        private readonly Vector2Int targetCoordinate;

        private readonly List<WorldPathAction> path = new();

        private int pathIndex = 0;

        private float actionProgress = 0f;

        public Vector2Int TargetCoordinate => targetCoordinate;

        public IReadOnlyList<WorldPathAction> Path => path;

        public int PathIndex => pathIndex;

        public bool hasPath = false;

        public bool IsPathComplete => hasPath == true && pathIndex >= path.Count;

        public override int PriorityInJobQueue => 1000;


        public CreatureMoveJob(CreatureController controller, Vector2Int targetCoordinate) : base(controller)
        {
            this.targetCoordinate = targetCoordinate;

            if (WorldManager.TryGetWorldMap(out WorldMap map) == false)
            {
                hasPath = false;
                return;
            }

            Vector2Int startCoordinate = WorldManager.WorldToTileCoordinate(controller.transform.position);
            WorldPathMovementProfile movementProfile = controller.CreatePathMovementProfile();

            if (WorldPathfinder.TryFindPath(map, startCoordinate, targetCoordinate, movementProfile, out path) == false)
            {
                hasPath = false;
                return;
            }

            hasPath = true;
        }

        public void SetPathIndex(int index)
        {
            pathIndex = index;
        }

        public bool IsPathCompleted()
        {
            return hasPath == true && pathIndex >= path.Count;
        }

        public float GetActionProgress()
        {
            return actionProgress;
        }

        public WorldPathAction GetCurrentPathAction()
        {
            return path[pathIndex];
        }

        public void Advance()
        {
            pathIndex++;
            actionProgress = 0f;
        }

        public void AdvanceActionProgress(float amount)
        {
            actionProgress = Mathf.Clamp01(actionProgress + amount);
        }
    }

    public class CreatureWanderJob : CreatureJob
    {
        public CreatureWanderJob(CreatureController controller) : base(controller)
        {
        }
    }

    public class CreatureMiningJob : CreatureJob
    {
        public CreatureMiningJob(CreatureController controller) : base(controller)
        {
        }
    }

    public class CreatureEngageJob : CreatureJob
    {
        public CreatureEngageJob(CreatureController controller) : base(controller)
        {

        }
    }
}
