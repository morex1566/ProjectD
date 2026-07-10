using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    // Creature 전투 접근 및 사거리 판정 로직
    public partial class CreatureController
    {
        /// <summary>
        /// 전투 Job의 대상이 살아 있고 접근 가능한 상태인지 확인합니다.
        /// </summary>
        public bool IsEngageTargetValid(CreatureEngageJob engageJob)
        {
            CreatureController target = engageJob.Target;
            return target != null && target.IsDead() == false;
        }

        /// <summary>
        /// 전투 Job의 대상이 공격 사거리 안에 있는지 확인합니다.
        /// </summary>
        public bool IsInAttackRange(CreatureEngageJob engageJob)
        {
            CreatureController target = engageJob.Target;
            if (target == null)
            {
                return false;
            }

            float sqrDistance = (target.transform.position - transform.position).sqrMagnitude;
            return sqrDistance <= context.AttackRange * context.AttackRange;
        }

        /// <summary>
        /// 전투 Job에 이동 경로가 없거나 대상 위치가 바뀌었으면 경로를 갱신합니다.
        /// </summary>
        public bool TryEnsureEngagePath(CreatureEngageJob engageJob)
        {
            CreatureController target = engageJob.Target;
            if (target == null)
            {
                engageJob.ClearPath();
                return false;
            }

            Vector3Int targetCellPosition = WorldManager.WorldToCell(target.transform.position);
            if (engageJob.Path != null &&
                engageJob.Path.Count > 0 &&
                engageJob.PathIndex < engageJob.Path.Count &&
                engageJob.PathTargetCellPosition == targetCellPosition)
            {
                return true;
            }

            Vector3Int currentCellPosition = WorldManager.WorldToCell(transform.position);
            List<AStarNode> path = AStarPathfinder.FindPath(currentCellPosition, targetCellPosition);
            if (path == null || path.Count <= 0)
            {
                engageJob.ClearPath();
                return false;
            }

            engageJob.SetPath(targetCellPosition, path);
            return true;
        }

        /// <summary>
        /// 전투 Job의 접근 경로를 한 프레임 진행하고 완료 여부를 반환합니다.
        /// </summary>
        public bool MoveAlongEngageJob(CreatureEngageJob engageJob)
        {
            bool isArrived = MoveAlongPath(engageJob.Path, engageJob.PathIndex, out int nextPathIndex);
            engageJob.SetPathIndex(nextPathIndex);

            return isArrived;
        }
    }
}
