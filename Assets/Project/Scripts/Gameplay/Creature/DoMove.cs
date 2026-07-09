using UnityEngine;
using MBT;
using System.Collections.Generic;

namespace TRPG.Runtime
{
    [AddComponentMenu("")]
    [MBTNode(name = "Creature/Leaf - DoMove")]
    public class DoMove : Leaf
    {
        private const float ArriveDistance = 0.01f;

        [SerializeField, ReadOnly] private CreatureController controller = null;
        [SerializeField, ReadOnly] private int pathIndex = 1;

        private CreatureMoveJob currentMoveJob = null;
        private IReadOnlyList<AStarNode> path = null;

        private void OnValidate()
        {
            CacheComponents();
        }

        private void Awake()
        {
            CacheComponents();
        }

        public override NodeResult Execute()
        {
            if (controller.JobQueue.TryPeek(out CreatureJob job) == false || job is not CreatureMoveJob moveJob)
            {
                return NodeResult.failure;
            }

            if (currentMoveJob != moveJob)
            {
                currentMoveJob = moveJob;
                path = currentMoveJob.Path;
                pathIndex = currentMoveJob.PathIndex;
            }

            if (path == null || path.Count <= 0 || pathIndex >= path.Count)
            {
                return NodeResult.success;
            }

            WorldGridController gridController = WorldManager.GetWorldGridController();
            if (gridController == null || gridController.Grid == null)
            {
                return NodeResult.failure;
            }

            AStarNode targetNode = path[pathIndex];
            Vector3Int targetCellPosition = new Vector3Int(targetNode.X, targetNode.Y, 0);
            Vector3 targetWorldPosition = gridController.Grid.GetCellCenterWorld(targetCellPosition);
            Vector3 currentWorldPosition = controller.transform.position;
            float nextX = Mathf.MoveTowards(currentWorldPosition.x, targetWorldPosition.x, controller.Context.MoveSpeed * Time.deltaTime);
            controller.transform.position = new Vector3(nextX, currentWorldPosition.y, currentWorldPosition.z);

            if (Mathf.Abs(controller.transform.position.x - targetWorldPosition.x) <= ArriveDistance)
            {
                controller.transform.position = new Vector3(targetWorldPosition.x, currentWorldPosition.y, currentWorldPosition.z);
                pathIndex++;
                currentMoveJob.SetPathIndex(pathIndex);
            }

            if (pathIndex >= path.Count)
            {
                return NodeResult.success;
            }

            return NodeResult.running;
        }

        public override void OnExit()
        {
            // Leaf 실행이 끝나면 다음 이동 명령을 받을 수 있도록 실행 상태를 비웁니다.
            currentMoveJob = null;
            path = null;
            pathIndex = 1;

            base.OnExit();
        }

        public override void DrawGizmos()
        {
            if (controller == null)
            {
                CacheComponents();
            }

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

            IReadOnlyList<AStarNode> movePath = moveJob.Path;
            int movePathIndex = moveJob.PathIndex;

            if (movePath == null || movePath.Count == 0)
            {
                return;
            }

            if (movePathIndex >= movePath.Count)
            {
                return;
            }

            Color previousColor = Gizmos.color;

            // 현재 위치에서 남은 경로 노드까지 이어지는 이동 경로를 표시합니다.
            Gizmos.color = Color.cyan;
            Vector3 previousWorldPosition = controller.transform.position;

            for (int i = movePathIndex; i < movePath.Count; i++)
            {
                Vector3 nodeWorldPosition = GetNodeWorldPosition(gridController, movePath[i]);
                Gizmos.DrawLine(previousWorldPosition, nodeWorldPosition);

                previousWorldPosition = nodeWorldPosition;
            }

            // 각 경로 노드 위치를 확인할 수 있도록 작은 마커를 표시합니다.
            float markerSize = Mathf.Min(gridController.Grid.cellSize.x, gridController.Grid.cellSize.y) * 0.25f;
            Vector3 markerScale = Vector3.one * markerSize;

            for (int i = movePathIndex; i < movePath.Count; i++)
            {
                Vector3 nodeWorldPosition = GetNodeWorldPosition(gridController, movePath[i]);
                Gizmos.DrawWireCube(nodeWorldPosition, markerScale);
            }

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(GetNodeWorldPosition(gridController, movePath[movePath.Count - 1]), markerScale);

            Gizmos.color = previousColor;
        }

        private Vector3 GetNodeWorldPosition(WorldGridController gridController, AStarNode node)
        {
            Vector3Int cellPos = new Vector3Int(node.X, node.Y, 0);
            return gridController.Grid.GetCellCenterWorld(cellPos);
        }

        private void CacheComponents()
        {
            controller = GetComponentInParent<CreatureController>();
        }
    }
}
