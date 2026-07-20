using UnityEngine;
using MBT;
using System.Collections.Generic;

namespace TRPG.Runtime
{
    [AddComponentMenu("")]
    [MBTNode(name = "Creature/Leaf - DoMove")]
    public class DoMove : Leaf
    {
        [SerializeField, ReadOnly] private CreatureController controller = null;

        [Header("Debug")]
        [SerializeField] private bool drawPathGizmos = true;

        [SerializeField, Min(0.01f)] private float pathGizmoRadius = 0.12f;

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
            if (controller == null || controller.IsDead() == true || controller.Context.MoveSpeed <= 0f)
            {
                return NodeResult.failure;
            }

            if (controller.JobQueue.TryPeek(out CreatureMoveJob job) == false)
            {
                return NodeResult.failure;
            }

            if (job.hasPath == false)
            {
                job.Complete();
                return NodeResult.failure;
            }

            if (job.IsPathComplete == true)
            {
                job.Complete();
                return NodeResult.success;
            }

            // 여기서 이동 로직?
            WorldPathAction action = job.GetCurrentPathAction();

            return ExecuteCurrentAction(job, action);
        }

        /// <summary>
        /// 현재 경로 행동 종류에 맞는 이동 로직을 실행합니다.
        /// </summary>
        private NodeResult ExecuteCurrentAction(CreatureMoveJob moveJob, WorldPathAction action)
        {
            switch (action.Type)
            {
                case WorldPathActionType.Walk:
                    return ExecuteWalk(moveJob, action);

                case WorldPathActionType.Jump:
                    return ExecuteJump(moveJob, action);

                case WorldPathActionType.Fall:
                    return ExecuteFall(moveJob, action);

                // 아직 구현되지 않은 행동이 경로에 포함되면 Job을 실패 종료합니다.
                default:
                    moveJob.Complete();
                    return NodeResult.failure;
            }
        }

        private NodeResult ExecuteWalk(CreatureMoveJob moveJob, WorldPathAction action)
        {
            if (controller.Walk(action) == true)
            {
                moveJob.Advance();
            }

            return NodeResult.running;
        }

        private NodeResult ExecuteJump(CreatureMoveJob moveJob, WorldPathAction action)
        {
            float tileWorldSize = WorldManager.Settings.WorldGenerationSettingsData.TileWorldSize;
            float actionDistance = Mathf.Max(Vector2.Distance(action.From, action.To), tileWorldSize);
            float movementSpeed = controller.Context.MoveSpeed * tileWorldSize;
            float progressDelta = movementSpeed * Time.deltaTime / actionDistance;

            moveJob.AdvanceActionProgress(progressDelta);

            float actionProgress = moveJob.GetActionProgress();

            if (controller.Jump(action.From, action.To, actionProgress) == true)
            {
                moveJob.Advance();
            }

            return NodeResult.running;
        }

        private NodeResult ExecuteFall(CreatureMoveJob moveJob, WorldPathAction action)
        {
            Vector2 entryWorldPosition = CreatureController.GetFallEntryPosition(action);

            float tileWorldSize = WorldManager.Settings.WorldGenerationSettingsData.TileWorldSize;
            float actionDistance = Vector2.Distance(action.From, entryWorldPosition) + Vector2.Distance(entryWorldPosition, action.To);
            float movementSpeed = controller.Context.MoveSpeed * tileWorldSize;
            float progressDelta = movementSpeed * Time.deltaTime / Mathf.Max(actionDistance, tileWorldSize);

            moveJob.AdvanceActionProgress(progressDelta);

            float actionProgress = moveJob.GetActionProgress();

            if (controller.Fall(action, actionProgress) == true)
            {
                moveJob.Advance();
            }

            return NodeResult.running;
        }

        private void CacheComponents()
        {
            controller = GetComponentInParent<CreatureController>();
        }

        /// <summary>
        /// 현재 MoveJob의 완료, 실행 중, 예정 경로를 Scene 뷰에 표시합니다.
        /// </summary>
        private void OnDrawGizmos()
        {
            if (drawPathGizmos == false || WorldManager.Settings?.WorldGenerationSettingsData == null)
            {
                return;
            }

            CreatureController targetController = controller != null ? controller : GetComponentInParent<CreatureController>();
            if (targetController == null || targetController.JobQueue.TryPeek(out CreatureMoveJob moveJob) == false || moveJob.hasPath == false)
            {
                return;
            }

            Color previousColor = Gizmos.color;
            float worldRadius = pathGizmoRadius * WorldManager.Settings.WorldGenerationSettingsData.TileWorldSize;

            for (int i = 0; i < moveJob.Path.Count; i++)
            {
                WorldPathAction action = moveJob.Path[i];
                Gizmos.color = GetPathGizmoColor(action.Type, i, moveJob.PathIndex);
                DrawPathActionGizmo(action, worldRadius);
            }

            Gizmos.color = previousColor;
        }

        /// <summary>
        /// 행동 종류에 맞는 경로 모양을 표시합니다.
        /// </summary>
        private static void DrawPathActionGizmo(WorldPathAction action, float worldRadius)
        {
            Vector3 fromWorldPosition = action.From;
            Vector3 targetWorldPosition = action.To;

            Gizmos.DrawWireSphere(fromWorldPosition, worldRadius);

            switch (action.Type)
            {
                case WorldPathActionType.Jump:
                    DrawJumpGizmo(action);
                    break;

                case WorldPathActionType.Fall:
                    Vector3 entryWorldPosition = CreatureController.GetFallEntryPosition(action);
                    Gizmos.DrawLine(fromWorldPosition, entryWorldPosition);
                    Gizmos.DrawLine(entryWorldPosition, targetWorldPosition);
                    break;

                case WorldPathActionType.Walk:
                default:
                    Gizmos.DrawLine(fromWorldPosition, targetWorldPosition);
                    break;
            }

            Gizmos.DrawWireSphere(targetWorldPosition, worldRadius);
        }

        /// <summary>
        /// 점프 행동을 여러 선분으로 나눠 포물선 형태로 표시합니다.
        /// </summary>
        private static void DrawJumpGizmo(WorldPathAction action)
        {
            const int SegmentCount = 12;
            Vector3 previousWorldPosition = action.From;

            for (int i = 1; i <= SegmentCount; i++)
            {
                float ratio = i / (float)SegmentCount;
                Vector3 jumpWorldPosition = CreatureController.CalculateJumpPosition(action.From, action.To, ratio);
                Gizmos.DrawLine(previousWorldPosition, jumpWorldPosition);
                previousWorldPosition = jumpWorldPosition;
            }
        }

        /// <summary>
        /// 완료 경로는 회색, 현재 경로는 빨강, 예정 경로는 행동별 색으로 구분합니다.
        /// </summary>
        private static Color GetPathGizmoColor(WorldPathActionType actionType, int actionIndex, int currentPathIndex)
        {
            if (actionIndex < currentPathIndex)
            {
                return Color.gray;
            }

            if (actionIndex == currentPathIndex)
            {
                return Color.red;
            }

            return actionType switch
            {
                WorldPathActionType.Jump => Color.cyan,
                WorldPathActionType.Fall => Color.yellow,
                _ => Color.green,
            };
        }
    }
}
