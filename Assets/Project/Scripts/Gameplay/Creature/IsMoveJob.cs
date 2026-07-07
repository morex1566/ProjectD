using MBT;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    [AddComponentMenu("")]
    [MBTNode(name = "Creature/Condition - IsMoveJob")]
    public class IsMoveJob : Condition
    {
        [SerializeField, ReadOnly] private CreatureController controller = null;

        public override void OnEnter()
        {
            controller = GetComponentInParent<CreatureController>();

            base.OnEnter();
        }

        public override bool Check()
        {
            return controller.JobQueue.TryPeek(out CreatureJob job) == true && job is CreatureMoveJob;
        }

        public override void DrawGizmos()
        {
            controller ??= GetComponentInParent<CreatureController>();
            if (controller == null)
            {
                return;
            }

            if (Application.isPlaying == false)
            {
                return;
            }

            if (controller.JobQueue.TryPeek(out CreatureJob job) == false || job is not CreatureMoveJob moveJob)
            {
                return;
            }

            WorldGridController gridController = WorldManager.GetWorldGridController();
            if (gridController == null || gridController.Grid == null)
            {
                return;
            }

            IReadOnlyList<AStarNode> path = moveJob.Path;
            int pathIndex = moveJob.PathIndex;

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
            Vector3 previousWorldPosition = controller.transform.position;

            for (int i = pathIndex; i < path.Count; i++)
            {
                Vector3 nodeWorldPosition = GetNodeWorldPosition(gridController, path[i]);
                Gizmos.DrawLine(previousWorldPosition, nodeWorldPosition);

                previousWorldPosition = nodeWorldPosition;
            }

            // 각 경로 노드 위치를 확인할 수 있도록 작은 마커를 표시합니다.
            float markerSize = Mathf.Min(gridController.Grid.cellSize.x, gridController.Grid.cellSize.y) * 0.25f;
            Vector3 markerScale = Vector3.one * markerSize;

            for (int i = pathIndex; i < path.Count; i++)
            {
                Vector3 nodeWorldPosition = GetNodeWorldPosition(gridController, path[i]);
                Gizmos.DrawWireCube(nodeWorldPosition, markerScale);
            }

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(GetNodeWorldPosition(gridController, path[path.Count - 1]), markerScale);

            Gizmos.color = previousColor;
        }

        private Vector3 GetNodeWorldPosition(WorldGridController gridController, AStarNode node)
        {
            Vector3Int cellPos = new Vector3Int(node.X, node.Y, 0);
            return gridController.Grid.GetCellCenterWorld(cellPos);
        }
    }
}
