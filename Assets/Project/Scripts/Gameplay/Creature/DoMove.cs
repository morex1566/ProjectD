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
        private const float ArriveSqrDistance = ArriveDistance * ArriveDistance;

        [SerializeField, ReadOnly] private CreatureController controller = null;
        [SerializeField, ReadOnly] private int pathIndex = 1;

        private CreatureMoveJob currentMoveJob = null;
        private IReadOnlyList<AStarNode> path = null;

        public override void OnEnter()
        {
            // BT 노드는 Creature 하위에 있으므로 부모에서 런타임 컨트롤러를 찾습니다.
            controller = GetComponentInParent<CreatureController>();

            base.OnEnter();
        }

        public override NodeResult Execute()
        {
            // Running 중에는 같은 Job을 계속 들고 있어야 큐를 매 프레임 다시 소비하지 않습니다.
            if (TryStartMove(out NodeResult startResult) == false)
            {
                return startResult;
            }

            if (controller.Context.State.HasFlag(CreatureStateType.Dead) == true)
            {
                return NodeResult.failure;
            }

            if (controller.Context.MoveSpeed <= 0f)
            {
                return NodeResult.failure;
            }

            WorldGridContext gridContext = WorldManager.GetWorldGridContext();
            if (gridContext == null)
            {
                return NodeResult.failure;
            }

            if (pathIndex >= path.Count)
            {
                return NodeResult.success;
            }

            // 현재 A* 노드의 셀 중앙을 향해 이번 프레임에 이동할 목표 좌표를 계산합니다.
            Vector3 targetWorldPos = GetNodeWorldPos(gridContext, path[pathIndex]);
            targetWorldPos.z = controller.transform.position.z;

            float moveDistance = controller.Context.MoveSpeed * Time.deltaTime;
            controller.transform.position = Vector3.MoveTowards(controller.transform.position, targetWorldPos, moveDistance);

            // 아직 목표 노드에 도착하지 않았으면 다음 Tick에서 이어서 이동합니다.
            if (Vector3.SqrMagnitude(controller.transform.position - targetWorldPos) > ArriveSqrDistance)
            {
                return NodeResult.running;
            }

            // 목표 노드에 도착하면 셀 중앙으로 보정하고 다음 경로 노드로 진행합니다.
            controller.transform.position = targetWorldPos;
            pathIndex++;
            currentMoveJob.SetPathIndex(pathIndex);

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
            if (Application.isPlaying == false)
            {
                return;
            }

            if (controller == null || currentMoveJob == null || path == null)
            {
                return;
            }

            WorldGridContext gridContext = WorldManager.GetWorldGridContext();
            if (gridContext == null)
            {
                return;
            }

            if (path.Count == 0 || pathIndex >= path.Count)
            {
                return;
            }

            Color previousColor = Gizmos.color;

            // 실행 중인 이동 잡의 남은 경로를 표시합니다.
            Gizmos.color = Color.cyan;
            Vector3 previousWorldPos = controller.transform.position;

            for (int i = pathIndex; i < path.Count; i++)
            {
                Vector3 nodeWorldPos = GetNodeWorldPos(gridContext, path[i]);
                Gizmos.DrawLine(previousWorldPos, nodeWorldPos);

                previousWorldPos = nodeWorldPos;
            }

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

        private bool TryStartMove(out NodeResult result)
        {
            result = NodeResult.failure;

            // MoveCreature는 큐 맨 앞의 이동 Job만 소비합니다.
            if (controller.JobQueue.TryPeek(out CreatureJob job) == false || job is not CreatureMoveJob moveJob)
            {
                return false;
            }

            currentMoveJob = moveJob;
            path = moveJob.Path;
            pathIndex = Mathf.Max(1, moveJob.PathIndex);

            // 경로 생성 실패는 이동 실패로 처리합니다.
            // 시작 노드만 있거나 이미 끝난 경로라면 실제 이동 없이 성공 처리합니다.
            if (path == null || path.Count <= 1 || pathIndex >= path.Count)
            {
                result = NodeResult.success;
                return false;
            }

            currentMoveJob.SetPathIndex(pathIndex);
            result = NodeResult.running;
            return true;
        }

        private Vector3 GetNodeWorldPos(WorldGridContext gridContext, AStarNode node)
        {
            Vector3Int cellPos = new Vector3Int(node.X, node.Y, 0);
            return gridContext.Grid.GetCellCenterWorld(cellPos);
        }
    }
}
