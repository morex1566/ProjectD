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
        protected bool isDone;

        protected bool isStarted;

        protected CreatureController controller;

        public bool IsDone => isDone;


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
                isStarted = Start();
            }

            if (isStarted == false)
            {
                return false;
            }

            isDone = Update();

            return isDone;
        }

        /// <summary>
        /// 이 잡이 실행될 수 있는지?
        /// </summary>
        protected abstract bool Start();

        /// <summary>
        /// 이 잡이 끝났는지?
        /// </summary>
        protected abstract bool Update();

        public virtual void DrawGizmos() { }
    }

    public class CreatureMoveJob : CreatureJob
    {
        private readonly Vector3Int targetCellPos;

        private readonly float stopDistance = 0.05f;

        private List<AStarNode> path;

        public CreatureMoveJob(CreatureController owner, Vector3Int targetCellPos) : base(owner)
        {
            this.targetCellPos = targetCellPos;

            // 길찾기
            Vector3Int worldCellPos = WorldManager.WorldToCell(owner.transform.position);
            path = AStarPathfinder.FindPath(worldCellPos, targetCellPos);
        }

        protected override bool Start()
        {
            // 죽으면 못움직이지...
            if (controller.StateMahcine.CurrentStates.ContainsKey(CreatureStateType.Dead) == true)
            {
                return false;
            }

            return true;
        }

        protected override bool Update()
        {
            return true;
        }
    }
}