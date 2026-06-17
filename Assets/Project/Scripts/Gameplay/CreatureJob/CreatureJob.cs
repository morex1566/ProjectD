using Mono.Cecil.Cil;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    public abstract class CreatureJob
    {
        public int Priority;

        public bool IsDone;

        protected CreatureController owner;

        protected CreatureJobMachine queue;

        protected CreatureJob(CreatureController owner, CreatureJobMachine queue, int priority)
        {
            this.owner = owner;
            this.queue = queue;
            Priority = priority;
        }

        public virtual void Execute()
        {
            IsDone = Evaluate();
        }

        /// <summary>
        /// IsDone 되었는지?
        /// </summary>
        public abstract bool Evaluate();

        public virtual void DrawGizmos()
        {

        }
    }

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

            // 마우스로 클릭한 지점에서 공중이라면 그 아래로 탐색하면서 지표면이 될때까지 보정
            var targetPosInt = WorldManager.Map.Ground.WorldToCell(targetPos);
            if (WorldManager.Map.GetTileType(targetPosInt.x, targetPosInt.y).HasFlag(MapTileType.Air))
            {
                while (WorldManager.Map.GetTileType(targetPosInt.x, targetPosInt.y - 1).HasFlag(MapTileType.Air))
                {
                    targetPosInt.y -= 1;
                }
            }

            // 마우스로 클릭한 지점이 땅이라면 그 위로 탐색하면서 지표면위의 에어가 될때까지 보정
            if (WorldManager.Map.GetTileType(targetPosInt.x, targetPosInt.y).HasFlag(MapTileType.Ground) ||
                WorldManager.Map.GetTileType(targetPosInt.x, targetPosInt.y).HasFlag(MapTileType.GroundSurface))
            {
                while (!WorldManager.Map.GetTileType(targetPosInt.x, targetPosInt.y).HasFlag(MapTileType.Air))
                {
                    targetPosInt.y += 1;
                }
            }

            path = AStarPathfinder.FindPath(startPos, targetPosInt);
            SkipStartNode(startPos);
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
        public override bool Evaluate()
        {
            if (owner == null) return true;
            if (path == null || path.Count == 0) return true;

            return isDone;
        }

        public override void DrawGizmos()
        {
            if (path == null || path.Count == 0)
            {
                return;
            }

            Color prevColor = Gizmos.color;
            Gizmos.color = Color.green;

            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector3 from = GetNodeWorldPos(path[i]);
                Vector3 to = GetNodeWorldPos(path[i + 1]);

                // 경로 노드끼리 선을 이어줍니다.
                Gizmos.DrawLine(from, to);

                // 노드 위치를 작은 구체로 표시합니다.
                Gizmos.DrawSphere(from, 0.05f);
            }

            // 마지막 노드도 표시합니다.
            Gizmos.DrawSphere(GetNodeWorldPos(path[^1]), 0.05f);

            Gizmos.color = prevColor;
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
            Vector3Int cellPos = new Vector3Int(node.X, node.Y, 0);

            // Tilemap 셀의 중심 월드 좌표를 가져옵니다.
            Vector3 worldPos = WorldManager.Map.Ground.GetCellCenterWorld(cellPos);

            // 2D 기준으로 Z값을 고정합니다.
            worldPos.z = owner != null ? owner.transform.position.z : 0f;

            return worldPos;
        }

        /// <summary>
        /// A* 결과의 첫 노드는 시작 셀이므로 실제 이동 목표에서는 제외합니다.
        /// </summary>
        private void SkipStartNode(Vector3Int startPos)
        {
            if (path == null || path.Count == 0)
            {
                return;
            }

            AStarNode firstNode = path[0];
            if (firstNode.X == startPos.x && firstNode.Y == startPos.y)
            {
                pathIndex = 1;
            }
        }
    }

    public class CreatureDigJob : CreatureJob
    {
        private readonly DigSystem digSystem;

        private readonly float digSpeed = 25f;

        private DigAction digAction;

        /// <summary>
        /// 아마 땅파는 지점이 멀리에 있으면
        /// </summary>
        private CreatureMoveJob moveJob;

        public CreatureDigJob(DigSystem digSystem, CreatureController owner, CreatureJobMachine queue, int priority) : base(owner, queue, priority)
        {
            this.digSystem = digSystem;
        }

        public override bool Evaluate()
        {
            return digAction != null && digAction.IsCompleted;
        }

        public override void Execute()
        {
            // 땅파기를 성공적으로 마침
            if (digAction == null)
            {
                if (!digSystem.TryGetNextAction(out digAction))
                {
                    IsDone = true;
                    return;
                }

                Vector3 accessWorldPos = GetAccessWorldPos(digAction.CellPos);
                moveJob = new CreatureMoveJob(accessWorldPos, owner.Status.MoveSpeed, owner, queue, Priority);
            }

            // 아직 땅파기 지점으로 가지 못했나?
            if (!moveJob.IsDone)
            {
                moveJob.Execute();
                return;
            }

            Dig();

            if (digAction.IsCompleted)
            {
                digSystem.CompleteAction(digAction);
                IsDone = true;
            }
        }

        private void Dig()
        {
            digAction.AddProgress(digSpeed * Time.deltaTime);
        }

        private Vector3 GetAccessWorldPos(Vector3Int targetCellPos)
        {
            Vector3Int accessCellPos = targetCellPos + Vector3Int.up;
            return WorldManager.Map.Ground.GetCellCenterWorld(accessCellPos);
        }
    }
}
