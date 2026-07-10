using System;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// Creature가 순차 실행하는 작업 단위의 공통 베이스입니다.
    /// </summary>
    public abstract class CreatureJob
    {
        /// <summary>
        /// 클수록 중요하다.
        /// 같으면 CreatureQueue의 먼저 들어온를 기준으로
        /// </summary>
        public virtual int Priority => 0;

        protected bool isDone;

        protected bool isStarted;

        protected CreatureController controller;

        public event Action<CreatureJob> Completed;

        public bool IsDone => isDone;

        /// <summary>
        /// 아직 할당 안받음용
        /// </summary>
        protected CreatureJob()
        {
        }

        /// <summary>
        /// 완료 조건을 생성
        /// </summary>
        /// <param name="controller"></param>
        protected CreatureJob(CreatureController controller)
        {
            SetCreatureController(controller);
        }

        public void SetCreatureController(CreatureController controller)
        {
            this.controller = controller;
        }

        /// <summary>
        /// 작업이 끝났음을 알리고, 이 Job을 구독 중인 큐가 즉시 제거할 수 있게 합니다.
        /// </summary>
        public void Complete()
        {
            if (isDone == true)
            {
                return;
            }

            isStarted = true;
            isDone = true;
            Completed?.Invoke(this);
        }
    }

    public class CreatureMoveJob : CreatureJob
    {
        public override int Priority => 1000;

        private readonly Vector3Int targetCellPos;

        private List<AStarNode> path;

        private int pathIndex = 1;

        public IReadOnlyList<AStarNode> Path => path;

        public int PathIndex => pathIndex;


        public CreatureMoveJob(CreatureController controller, Vector3Int targetCellPos) : base(controller)
        {
            this.targetCellPos = targetCellPos;

            // 길찾기
            Vector3Int worldCellPos = WorldManager.WorldToCell(controller.transform.position);
            path = AStarPathfinder.FindPath(worldCellPos, targetCellPos);

            // 방황하던 로직 삭제
            controller.JobQueue.RemoveWhere(job => job is CreatureWanderJob == true);
        }


        /// <summary>
        /// 이동 실행 노드가 현재 진행 중인 경로 인덱스를 Job에 동기화합니다.
        /// </summary>
        public void SetPathIndex(int pathIndex)
        {
            this.pathIndex = pathIndex;
        }
    }

    public class CreatureMiningJob : CreatureJob
    {
        public override int Priority => 10;

        private readonly Vector3Int targetCellPosition = Vector3Int.zero;

        private readonly List<CreatureController> workers = new();

        private List<AStarNode> path = null;

        private int pathIndex = 1;


        public Vector3Int TargetCellPosition => targetCellPosition;

        public IReadOnlyList<CreatureController> Workers => workers;

        public IReadOnlyList<AStarNode> Path => path;

        public int PathIndex => pathIndex;


        /// <summary>
        /// 아직 Creature에게 배정되지 않은 채굴 작업을 생성합니다.
        /// </summary>
        public CreatureMiningJob(Vector3Int targetCellPosition)
        {
            this.targetCellPosition = targetCellPosition;
        }

        public CreatureMiningJob(CreatureController controller, Vector3Int targetCellPosition) : base(controller)
        {
            this.targetCellPosition = targetCellPosition;

            // 방황하던 로직 삭제
            controller.JobQueue.RemoveWhere(job => job is CreatureWanderJob == true);
        }

        /// <summary>
        /// 채굴 지점까지 이동하기 위한 경로를 Job에 저장합니다.
        /// </summary>
        public void SetPath(IReadOnlyList<AStarNode> path)
        {
            this.path = path == null ? null : new List<AStarNode>(path);
            pathIndex = 1;
        }

        /// <summary>
        /// DoMining이 현재 진행 중인 경로 인덱스를 Job에 동기화합니다.
        /// </summary>
        public void SetPathIndex(int pathIndex)
        {
            this.pathIndex = pathIndex;
        }

        /// <summary>
        /// 현재 채굴 이동 경로를 초기화합니다.
        /// </summary>
        public void ClearPath()
        {
            path = null;
            pathIndex = 1;
        }
    }

    public class CreatureEngageJob : CreatureJob
    {
        public override int Priority => 100;

        private readonly CreatureController target = null;

        private List<AStarNode> path = null;

        private int pathIndex = 1;

        private Vector3Int pathTargetCellPosition = Vector3Int.zero;

        public CreatureController Target => target;

        public IReadOnlyList<AStarNode> Path => path;

        public int PathIndex => pathIndex;

        public Vector3Int PathTargetCellPosition => pathTargetCellPosition;


        public CreatureEngageJob(CreatureController controller, CreatureController target) : base(controller)
        {
            this.target = target;

            // 방황하던 로직 삭제
            controller.JobQueue.RemoveWhere(job => job is CreatureWanderJob == true);
        }

        /// <summary>
        /// 전투 대상에게 접근하기 위한 경로를 Job에 저장합니다.
        /// </summary>
        public void SetPath(Vector3Int pathTargetCellPosition, IReadOnlyList<AStarNode> path)
        {
            this.pathTargetCellPosition = pathTargetCellPosition;
            this.path = path == null ? null : new List<AStarNode>(path);
            pathIndex = 1;
        }

        /// <summary>
        /// DoEngage가 현재 진행 중인 경로 인덱스를 Job에 동기화합니다.
        /// </summary>
        public void SetPathIndex(int pathIndex)
        {
            this.pathIndex = pathIndex;
        }

        /// <summary>
        /// 현재 전투 접근 경로를 초기화합니다.
        /// </summary>
        public void ClearPath()
        {
            path = null;
            pathIndex = 1;
            pathTargetCellPosition = Vector3Int.zero;
        }

        private bool IsTargetValid()
        {
            if (target == null)
            {
                return false;
            }

            return target.IsDead() == false;
        }
    }

    /// <summary>
    /// 크리쳐의 기본이 되는 작업
    /// </summary>
    public class CreatureWanderJob : CreatureJob
    {
        public override int Priority => 1;

        private readonly List<Vector3> pathWorldPositions = new();

        private Vector3Int targetCellPosition = Vector3Int.zero;

        private float startTime = 0f;

        private int pathIndex = 1;

        private bool hasStarted = false;

        /// <summary>
        /// 현재 배회 Job이 목적지를 탐색할 좌우 반경입니다.
        /// </summary>
        public int WanderRadius { get; }

        /// <summary>
        /// 현재 배회 Job이 시작 전 대기할 시간입니다.
        /// </summary>
        public float StartDelaySec { get; }

        public IReadOnlyList<Vector3> PathWorldPositions => pathWorldPositions;

        public Vector3Int TargetCellPosition => targetCellPosition;

        public float StartTime => startTime;

        public int PathIndex => pathIndex;

        public bool HasStarted => hasStarted;

        /// <summary>
        /// 자동 Job 생성 시점의 배회 설정을 Job에 고정합니다.
        /// </summary>
        public CreatureWanderJob(CreatureController controller, int wanderRadius, float startDelaySec) : base(controller)
        {
            WanderRadius = wanderRadius;
            StartDelaySec = startDelaySec;
        }

        /// <summary>
        /// 배회 시작 시간을 기록합니다.
        /// </summary>
        public void Begin(float startTime)
        {
            this.startTime = startTime;
            hasStarted = true;
            pathIndex = 1;
            pathWorldPositions.Clear();
        }

        /// <summary>
        /// 배회 이동 경로를 Job에 저장합니다.
        /// </summary>
        public void SetPath(Vector3Int targetCellPosition, IReadOnlyList<Vector3> pathWorldPositions)
        {
            this.targetCellPosition = targetCellPosition;
            this.pathWorldPositions.Clear();
            this.pathWorldPositions.AddRange(pathWorldPositions);
            pathIndex = 1;
        }

        /// <summary>
        /// DoWander가 현재 진행 중인 경로 인덱스를 Job에 동기화합니다.
        /// </summary>
        public void SetPathIndex(int pathIndex)
        {
            this.pathIndex = pathIndex;
        }

        /// <summary>
        /// 현재 배회 상태를 초기화합니다.
        /// </summary>
        public void ClearWanderState()
        {
            pathWorldPositions.Clear();
            targetCellPosition = Vector3Int.zero;
            startTime = 0f;
            pathIndex = 1;
            hasStarted = false;
        }
    }

}
