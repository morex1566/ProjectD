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

        private int pathIndex = 1;

        public CreatureMoveJob(CreatureController controller, Vector3Int targetCellPos) : base(controller)
        {
            this.targetCellPos = targetCellPos;
        }

        protected override bool Start()
        {
            // 죽으면 못움직이지...
            if (controller.StateMahcine.CurrentStates.ContainsKey(CreatureStateType.Dead) == true)
            {
                return false;
            }

            // 길찾기
            Vector3Int worldCellPos = WorldManager.WorldToCell(controller.transform.position);
            path = AStarPathfinder.FindPath(worldCellPos, targetCellPos);

            return true;
        }

        protected override bool Update()
        {
            WorldGridContext gridContext = WorldManager.GetWorldGridContext();
            if (gridContext == null)
            {
                return true;
            }

            if (path == null || path.Count == 0)
            {
                return true;
            }

            if (pathIndex >= path.Count)
            {
                return true;
            }

            AStarNode nextNode = path[pathIndex];
            Vector3Int nextCellPos = new Vector3Int(nextNode.X, nextNode.Y, 0);
            Vector3 nextWorldPos = gridContext.Grid.GetCellCenterWorld(nextCellPos);

            float currentX = controller.transform.position.x;
            float targetX = nextWorldPos.x;
            float distanceX = targetX - currentX;

            // Node에 도착
            if (Mathf.Abs(distanceX) <= stopDistance)
            {
                pathIndex++;
                return pathIndex >= path.Count;
            }

            // 이번 프레임에 목표 x까지 갈 수 있으면 스냅, 멀면 방향대로 이동
            // distanceX가 양수면 목표가 오른쪽에 있다는 뜻이라 directionX = 1
            // distanceX가 음수면 목표가 왼쪽에 있다는 뜻이라 directionX = -1
            float directionX = Mathf.Sign(distanceX);
            float moveDistance = controller.Context.MoveSpeed * Time.deltaTime;

            if (Mathf.Abs(distanceX) <= moveDistance)
            {
                Vector3 position = controller.transform.position;
                position.x = targetX;
                controller.transform.position = position;

                pathIndex++;
                return pathIndex >= path.Count;
            }

            controller.transform.position += Vector3.right * directionX * moveDistance;

            return false;
        }

        public override void DrawGizmos()
        {
            WorldGridContext gridContext = WorldManager.GetWorldGridContext();
            if (gridContext == null)
            {
                return;
            }

            if (path == null || path.Count == 0)
            {
                return;
            }

            if (pathIndex >= path.Count)
            {
                return;
            }

            Color previousColor = Gizmos.color;

            // 현재 위치에서 남은 경로 노드까지 이어지는 이동 경로를 표시합니다.
            Gizmos.color = Color.cyan;
            Vector3 previousWorldPos = controller.transform.position;

            for (int i = pathIndex; i < path.Count; i++)
            {
                Vector3 nodeWorldPos = GetNodeWorldPos(gridContext, path[i]);
                Gizmos.DrawLine(previousWorldPos, nodeWorldPos);

                previousWorldPos = nodeWorldPos;
            }

            // 각 경로 노드 위치를 확인할 수 있도록 작은 마커를 표시합니다.
            float markerSize = Mathf.Min(gridContext.Grid.cellSize.x, gridContext.Grid.cellSize.y) * 0.25f;
            Vector3 markerScale = Vector3.one * markerSize;

            for (int i = pathIndex; i < path.Count; i++)
            {
                Vector3 nodeWorldPos = GetNodeWorldPos(gridContext, path[i]);
                Gizmos.DrawWireCube(nodeWorldPos, markerScale);
            }

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(GetNodeWorldPos(gridContext, path[path.Count - 1]), markerScale);

            Gizmos.color = previousColor;
        }

        private Vector3 GetNodeWorldPos(WorldGridContext gridContext, AStarNode node)
        {
            Vector3Int cellPos = new Vector3Int(node.X, node.Y, 0);
            return gridContext.Grid.GetCellCenterWorld(cellPos);
        }
    }
}
