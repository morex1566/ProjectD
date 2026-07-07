using MBT;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    [AddComponentMenu("")]
    [MBTNode(name = "Creature/Leaf - DoWander")]
    public class DoWander : Leaf
    {
        [SerializeField, ReadOnly] private CreatureController controller = null;
        [SerializeField, ReadOnly] private Vector3Int targetCellPos = Vector3Int.zero;
        [SerializeField] private int wanderRadius = 2;
        [SerializeField] private float retryInterval = 1f;

        private float nextPickTime = 0f;

        private void Awake()
        {
            controller = GetComponentInParent<CreatureController>();
        }

        public override NodeResult Execute()
        {
            // 이미 이동 Job이 있으면 새 Wander 목적지를 만들지 않습니다.
            if (Time.time < nextPickTime)
            {
                return NodeResult.running;
            }

            if (TryPickReachableCell(out targetCellPos) == false)
            {
                nextPickTime = Time.time + retryInterval;
                return NodeResult.failure;
            }

            // 실제 배회 목적지 생성 전까지 Wander Job을 유지합니다.
            return NodeResult.running;
        }

        private bool TryPickReachableCell(out Vector3Int result)
        {
            result = Vector3Int.zero;

            Vector3Int currentCellPos = WorldManager.WorldToCell(controller.transform.position);

            //for (int i = 0; i < maxPickCount; i++)
            //{
            //    Vector2Int offset = new Vector2Int(
            //        Random.Range(-wanderRadius, wanderRadius + 1),
            //        Random.Range(-wanderRadius, wanderRadius + 1)
            //    );

            //    if (offset == Vector2Int.zero)
            //    {
            //        continue;
            //    }

            //    Vector3Int candidateCellPos = currentCellPos + new Vector3Int(offset.x, offset.y, 0);
            //    List<AStarNode> path = AStarPathfinder.FindPath(currentCellPos, candidateCellPos);

            //    // 자기 자신만 포함된 경로는 실제 이동이 없으므로 제외합니다.
            //    if (path == null || path.Count <= 1)
            //    {
            //        continue;
            //    }

            //    result = candidateCellPos;
            //    return true;
            //}

            return false;
        }

        public override void DrawGizmos()
        {
            if (Application.isPlaying == false)
            {
                return;
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(WorldManager.CellToWorld(targetCellPos), Vector3.one * 0.25f);
        }
    }
}
