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

        public virtual void DrawGizmos() { }
    }

    public class CreatureMoveJob : CreatureJob
    {
        private readonly Vector3Int targetCellPos;

        private List<AStarNode> path;

        private int pathIndex = 1;

        public CreatureMoveJob(CreatureController controller, Vector3Int targetCellPos) : base(controller)
        {
            this.targetCellPos = targetCellPos;

            // 길찾기
            Vector3Int worldCellPos = WorldManager.WorldToCell(controller.transform.position);
            path = AStarPathfinder.FindPath(worldCellPos, targetCellPos);
        }

        protected override bool CanStart()
        {
            // 죽으면 못움직이지...
            if (controller.Context.State.HasFlag(CreatureStateType.Dead) == true)
            {
                return false;
            }

            // 이미 도착한거 아님?
            if (path.Count <= 0)
            {
                return true;
            }

            return true;
        }

        protected override bool CanExit()
        {
            // 이미 도착한거 아님?
            if (path.Count <= 0)
            {
                return true;
            }

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
