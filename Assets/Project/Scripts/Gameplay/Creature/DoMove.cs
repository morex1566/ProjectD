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
                case WorldPathActionType.Fall:
                default:
                    // 아직 구현되지 않은 행동이 경로에 포함되면 Job을 실패 종료합니다.
                    moveJob.Complete();
                    return NodeResult.failure;
            }
        }

        /// <summary>
        /// 현재 위치에서 Walk 행동의 도착 셀까지 Creature를 이동시킵니다.
        /// </summary>
        private NodeResult ExecuteWalk(CreatureMoveJob moveJob, WorldPathAction action)
        {
            Vector3 currentWorldPosition = controller.transform.position;
            Vector3 targetWorldPosition = WorldManager.TileToWorldPosition(action.To);

            // MoveSpeed는 초당 타일 수로 사용합니다.
            float movementDistance = controller.Context.MoveSpeed * WorldManager.Settings.WorldGenerationSettingsData.TileWorldSize * Time.deltaTime;

            // 이동
            Vector3 nextWorldPosition = Vector3.MoveTowards(currentWorldPosition, targetWorldPosition, movementDistance);
            controller.transform.position = nextWorldPosition;

            if (Vector2.Distance(controller.transform.position, targetWorldPosition) < 0.001f)
            {
                moveJob.Advance();
            }

            return NodeResult.running;
        }

        private void CacheComponents()
        {
            controller = GetComponentInParent<CreatureController>();
        }
    }
}
