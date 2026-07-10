using MBT;
using UnityEngine;

namespace TRPG.Runtime
{
    [AddComponentMenu("")]
    [MBTNode(name = "Creature/Leaf - DoWander")]
    public class DoWander : Leaf
    {
        [SerializeField, ReadOnly] private CreatureController controller = null;

        [SerializeField, ReadOnly] private CreatureDetector detector = null;

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

            if (controller.JobQueue.TryPeek<CreatureWanderJob>(out CreatureWanderJob wanderJob) == false)
            {
                return NodeResult.failure;
            }

            if (detector.IsEnemyDetected(out _) == true)
            {
                return NodeResult.failure;
            }

            if (controller.TryExecuteWanderJob(wanderJob, out bool isCompleted) == false)
            {
                wanderJob.Complete();
                wanderJob.ClearWanderState();
                return NodeResult.failure;
            }


            if (isCompleted == true)
            {
                wanderJob.Complete();
                wanderJob.ClearWanderState();
                return NodeResult.success;
            }

            return NodeResult.running;
        }

        public override void DrawGizmos()
        {
            if (Application.isPlaying == false)
            {
                return;
            }

            if (controller == null)
            {
                return;
            }

            Gizmos.color = Color.yellow;
            WorldGridController gridController = WorldManager.GetWorldGridController();
            if (gridController == null || gridController.Grid == null)
            {
                return;
            }

            if (controller.JobQueue.TryPeek(out CreatureWanderJob wanderJob) == false)
            {
                return;
            }

            Gizmos.DrawWireCube(gridController.Grid.GetCellCenterWorld(wanderJob.TargetCellPosition), Vector3.one * 0.25f);
        }

        private void CacheComponents()
        {
            controller = gameObject.GetComponentInHierarchy<CreatureController>();
            detector = gameObject.GetComponentInHierarchy<CreatureDetector>();
        }
    }
}
