using Mono.Cecil.Cil;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// Creature가 순차 실행하는 작업 단위의 공통 베이스입니다.
    /// </summary>
    public abstract class CreatureJob
    {
        public virtual int Priority => 100;

        protected bool isDone;

        protected bool isStarted;

        protected CreatureController controller;

        public bool IsDone => isDone;

        /// <summary>
        /// 완료 조건을 생성
        /// </summary>
        /// <param name="controller"></param>
        protected CreatureJob(CreatureController controller)
        {
            this.controller = controller;
        }

        /// <summary>
        /// True면 다음 Job으로 넘어갈 수 있음, False면 아직 이 작업이 끝나지 않았음.
        /// </summary>
        /// <returns></returns>
        public bool Evaluate()
        {
            if (isStarted == false)
            {
                isStarted = CanStart();
            }

            if (isStarted == false)
            {
                return false;
            }

            isDone = CanExit();

            return isDone;
        }

        /// <summary>
        /// 이 잡이 실행될 수 있는지?
        /// </summary>
        protected abstract bool CanStart();

        /// <summary>
        /// 이 잡이 끝났는지?
        /// </summary>
        protected abstract bool CanExit();
    }

    public class CreatureMoveJob : CreatureJob
    {
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
        }


        /// <summary>
        /// 이동 실행 노드가 현재 진행 중인 경로 인덱스를 Job에 동기화합니다.
        /// </summary>
        public void SetPathIndex(int pathIndex)
        {
            this.pathIndex = pathIndex;
        }

        protected override bool CanStart()
        {
            // 죽으면 못움직이지...
            if (controller.Context.State.HasFlag(CreatureStateType.Dead) == true)
            {
                return false;
            }

            // 이미 도착한거 아님?
            if (path == null || path.Count <= 0)
            {
                return true;
            }

            return true;
        }

        protected override bool CanExit()
        {
            // 이미 도착한거 아님?
            if (path == null || path.Count <= 0 || pathIndex >= path.Count)
            {
                return true;
            }

            return false;
        }
    }

    public class CreatureMiningJob : CreatureJob
    {
        [SerializeField] private List<Vector3Int> targetCellPoss;

        public CreatureMiningJob(CreatureController controller, List<Vector3Int> targetCellPoss) : base(controller)
        {
            this.targetCellPoss = new List<Vector3Int>(targetCellPoss);
        }

        protected override bool CanExit()
        {
            // 죽으면 못움직이지...
            if (controller.Context.State.HasFlag(CreatureStateType.Dead) == true)
            {
                return false;
            }

            return true;
        }

        protected override bool CanStart()
        {
            throw new System.NotImplementedException();
        }
    }

    public class CreatureAttackJob : CreatureJob
    {
        public CreatureAttackJob(CreatureController controller) : base(controller)
        {
        }

        protected override bool CanExit()
        {
            throw new System.NotImplementedException();
        }

        protected override bool CanStart()
        {
            throw new System.NotImplementedException();
        }
    }

    /// <summary>
    /// 크리쳐의 기본이 되는 작업
    /// </summary>
    public class CreatureWanderJob : CreatureJob
    {
        public override int Priority =>  10;

        public CreatureWanderJob(CreatureController controller) : base(controller)
        {

        }

        protected override bool CanExit()
        {
            if (controller.IsDead() == true)
            {
                return true;
            }

            return false;
        }

        protected override bool CanStart()
        {
            // 죽으면 못움직이지...
            if (controller.IsDead() == true)
            {
                return false;
            }

            return true;
        }
    }
}
