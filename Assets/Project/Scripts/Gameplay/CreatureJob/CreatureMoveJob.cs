using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// AStar 경로를 따라 Creature를 이동시키는 Job입니다.
    /// </summary>
    public class CreatureMoveJob : CreatureJob
    {
        /// <summary>
        /// 노드 도착으로 판정할 거리입니다.
        /// </summary>
        public static float distanceThreshold = 0.05f;

        /// <summary>
        /// 이동할 AStar 경로입니다.
        /// </summary>
        private readonly List<AStarNode> path;

        /// <summary>
        /// 이동 속도입니다.
        /// </summary>
        private readonly float moveSpeed;

        /// <summary>
        /// 현재 따라가고 있는 경로 인덱스입니다.
        /// </summary>
        private int pathIndex = 0;

        /// <summary>
        /// 현재 프레임의 이동 방향입니다.
        /// </summary>
        private Vector3 direction;

        /// <summary>
        /// 이동이 완료되었는지 여부입니다.
        /// </summary>
        private bool isDone = false;

        /// <summary>
        /// AStar 경로 이동 Job을 생성합니다.
        /// </summary>
        public CreatureMoveJob(Vector3 targetPos, float moveSpeed, CreatureController owner, CreatureJobMachine queue, int priority) : base(owner, queue, priority)
        {
            this.moveSpeed = moveSpeed;

            // worldpos를 tilemap의 cellpos로 전환
            var startPos = WorldManager.Map.Ground.WorldToCell(owner.transform.position);
            var targetPosInt = WorldManager.Map.Ground.WorldToCell(targetPos);
            path = AStarPathfinder.FindPath(startPos, targetPosInt);
        }

        /// <summary>
        /// 이동 Job을 실행합니다.
        /// </summary>
        public override void Execute()
        {
            base.Execute();

            if (owner == null)
            {
                isDone = true;
                return;
            }

            Move();
        }

        /// <summary>
        /// 이동 Job이 완료되었는지 확인합니다.
        /// </summary>
        public override bool EvaluteIsDone()
        {
            if (owner == null) return true;
            if (path == null || path.Count == 0) return true;

            return isDone;
        }

        /// <summary>
        /// 경로를 따라 이동합니다.
        /// </summary>
        private void Move()
        {
            if (path == null || path.Count == 0)
            {
                isDone = true;
                return;
            }

            direction = Vector3.zero;

            float moveDistance = moveSpeed * Time.deltaTime;
            Vector3 currentPos = owner.transform.position;

            while (moveDistance > 0f && pathIndex < path.Count)
            {
                Vector3 targetPos = GetNodeWorldPos(path[pathIndex]);
                Vector3 toTarget = targetPos - currentPos;
                float distance = toTarget.magnitude;

                // 이미 현재 노드에 도착한 상태면 다음 노드로 넘어갑니다.
                if (distance <= distanceThreshold)
                {
                    currentPos = targetPos;
                    pathIndex++;
                    continue;
                }

                // 이번 프레임에 현재 노드까지 도착할 수 있는 경우입니다.
                if (moveDistance >= distance)
                {
                    currentPos = targetPos;
                    direction = toTarget.normalized;
                    moveDistance -= distance;
                    pathIndex++;
                    continue;
                }

                // 이번 프레임에 현재 노드까지 도착하지 못하는 경우입니다.
                Vector3 delta = toTarget.normalized * moveDistance;
                currentPos += delta;
                direction = delta.normalized;
                moveDistance = 0f;
            }

            owner.transform.position = currentPos;

            if (pathIndex >= path.Count)
            {
                isDone = true;
            }
        }

        /// <summary>
        /// AStarNode를 월드 좌표로 변환합니다.
        /// </summary>
        private Vector3 GetNodeWorldPos(AStarNode node)
        {
            Vector2 worldPos = new Vector2(node.X, node.Y);
            return worldPos;
        }
    }
}