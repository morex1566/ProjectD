using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    public class CreatureJob
    {
        public static float distanceThreshold = 0.05f;

        public static float MaxAttackGauge = 100f;

        public int Priority;

        public bool IsDone;

        private readonly CreatureJobType jobType;

        private readonly CreatureController owner;

        private readonly CreatureController target;

        private readonly List<AStarNode> path;

        private readonly float moveSpeed;

        private int pathIndex = 0;

        private Vector3 direction;

        private float attackGauge = 90f;

        private bool isMoveDone = false;

        private CreatureJob(CreatureJobType jobType, CreatureController owner, int priority, CreatureController target, List<AStarNode> path, float moveSpeed, int pathIndex)
        {
            this.jobType = jobType;
            this.owner = owner;
            this.target = target;
            this.path = path;
            this.moveSpeed = moveSpeed;
            this.pathIndex = pathIndex;
            Priority = priority;
        }

        public static CreatureJob CreateMove(Vector3 targetPos, float moveSpeed, CreatureController owner, int priority)
        {
            Vector3Int startPos = WorldManager.Map.Ground.WorldToCell(owner.transform.position);
            Vector3Int targetPosInt = WorldManager.Map.Ground.WorldToCell(targetPos);

            // 공중을 클릭한 경우, 아래쪽 지표면을 실제 이동 목표로 보정합니다.
            while (WorldManager.Map.GetTileType(targetPosInt.x, targetPosInt.y).HasFlag(MapTileType.Air) &&
                   WorldManager.Map.GetTileType(targetPosInt.x, targetPosInt.y - 1).HasFlag(MapTileType.Air))
            {
                targetPosInt.y -= 1;
            }

            List<AStarNode> path = AStarPathfinder.FindPath(startPos, targetPosInt);
            int pathIndex = GetStartPathIndex(path, startPos);

            return new CreatureJob(CreatureJobType.Move, owner, priority, null, path, moveSpeed, pathIndex);
        }

        public static CreatureJob CreateAttack(CreatureController target, CreatureController owner, int priority)
        {
            return new CreatureJob(CreatureJobType.Attack, owner, priority, target, null, 0f, 0);
        }

        public void Execute()
        {
            IsDone = EvaluteIsDone();
            if (IsDone)
            {
                return;
            }

            switch (jobType)
            {
                case CreatureJobType.Move:
                    Move();
                    break;
                case CreatureJobType.Attack:
                    Attack();
                    break;
            }

            IsDone = EvaluteIsDone();
        }

        public bool EvaluteIsDone()
        {
            switch (jobType)
            {
                case CreatureJobType.Move:
                    if (owner == null) return true;
                    if (path == null || path.Count == 0) return true;

                    return isMoveDone;
                case CreatureJobType.Attack:
                    return owner == null || target == null;
                default:
                    return true;
            }
        }

        private void Move()
        {
            if (path == null || path.Count == 0)
            {
                isMoveDone = true;
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
                isMoveDone = true;
            }
        }

        private void Attack()
        {
            if (!owner.Detector.Detect(target))
            {
                Vector3 currentPos = owner.transform.position;
                Vector3 nextPos = Vector3.MoveTowards(currentPos, target.transform.position, owner.Status.MoveSpeed * Time.deltaTime);

                direction = nextPos - currentPos;
                owner.transform.position = nextPos;

                return;
            }

            if (attackGauge <= MaxAttackGauge)
            {
                attackGauge += owner.Status.AttackSpeed * Time.deltaTime * 100f;
                return;
            }

            attackGauge = 0f;
            // TODO : 공격
        }

        private Vector3 GetNodeWorldPos(AStarNode node)
        {
            Vector2 worldPos = new Vector2(node.X, node.Y);
            return worldPos;
        }

        private static int GetStartPathIndex(List<AStarNode> path, Vector3Int startPos)
        {
            if (path == null || path.Count == 0)
            {
                return 0;
            }

            AStarNode firstNode = path[0];
            if (firstNode.X == startPos.x && firstNode.Y == startPos.y)
            {
                return 1;
            }

            return 0;
        }

        private enum CreatureJobType
        {
            Move,
            Attack
        }
    }
}
