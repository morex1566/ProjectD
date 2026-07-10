using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    // Creature 채굴 이동 및 실행 로직
    public partial class CreatureController
    {
        /// <summary>
        /// 채굴 Job에 이동 경로가 없으면 생성합니다.
        /// </summary>
        public bool TryEnsureMiningPath(CreatureMiningJob miningJob)
        {
            if (miningJob.Path != null && miningJob.Path.Count > 0 && miningJob.PathIndex < miningJob.Path.Count)
            {
                return true;
            }

            Vector3Int currentCellPosition = WorldManager.WorldToCell(transform.position);
            List<AStarNode> path = AStarPathfinder.FindPath(currentCellPosition, miningJob.TargetCellPosition);
            if (path == null || path.Count <= 0)
            {
                miningJob.ClearPath();
                return false;
            }

            miningJob.SetPath(path);
            return true;
        }

        /// <summary>
        /// 채굴 Job의 경로를 한 프레임 진행하고 완료 여부를 반환합니다.
        /// </summary>
        public bool MoveAlongMiningJob(CreatureMiningJob miningJob)
        {
            bool isArrived = MoveAlongPath(miningJob.Path, miningJob.PathIndex, out int nextPathIndex);
            miningJob.SetPathIndex(nextPathIndex);

            return isArrived;
        }
    }
}
