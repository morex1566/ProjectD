using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    // Creature 이동 실행 로직
    public partial class CreatureController
    {
        private const float ArriveDistance = 0.01f;

        /// <summary>
        /// 대상 월드 좌표의 X 위치까지 한 프레임 이동하고 도착 여부를 반환합니다.
        /// </summary>
        public bool MoveTowardsWorldPosition(Vector3 targetWorldPosition)
        {
            Vector3 currentWorldPosition = transform.position;
            float nextX = Mathf.MoveTowards(currentWorldPosition.x, targetWorldPosition.x, context.MoveSpeed * Time.deltaTime);
            transform.position = new Vector3(nextX, currentWorldPosition.y, currentWorldPosition.z);

            if (Mathf.Abs(transform.position.x - targetWorldPosition.x) > ArriveDistance)
            {
                return false;
            }

            transform.position = new Vector3(targetWorldPosition.x, currentWorldPosition.y, currentWorldPosition.z);
            return true;
        }

        /// <summary>
        /// AStar 경로와 현재 인덱스를 받아 한 프레임 이동하고, 갱신된 경로 인덱스를 반환합니다.
        /// </summary>
        public bool MoveAlongPath(IReadOnlyList<AStarNode> path, int pathIndex, out int nextPathIndex)
        {
            nextPathIndex = pathIndex;

            if (path == null || path.Count <= 0 || pathIndex >= path.Count)
            {
                return true;
            }

            if (TryGetNodeWorldPosition(path[pathIndex], out Vector3 targetWorldPosition) == false)
            {
                return false;
            }

            if (MoveTowardsWorldPosition(targetWorldPosition) == true)
            {
                nextPathIndex++;
            }

            return nextPathIndex >= path.Count;
        }

        /// <summary>
        /// 월드 좌표 경로와 현재 인덱스를 받아 한 프레임 이동하고, 갱신된 경로 인덱스를 반환합니다.
        /// </summary>
        public bool MoveAlongWorldPath(IReadOnlyList<Vector3> pathWorldPositions, int pathIndex, out int nextPathIndex)
        {
            nextPathIndex = pathIndex;

            if (pathWorldPositions == null || pathWorldPositions.Count <= 0 || pathIndex >= pathWorldPositions.Count)
            {
                return true;
            }

            if (MoveTowardsWorldPosition(pathWorldPositions[pathIndex]) == true)
            {
                nextPathIndex++;
            }

            return nextPathIndex >= pathWorldPositions.Count;
        }

        /// <summary>
        /// 이동 Job의 경로를 한 프레임 진행하고 완료 여부를 반환합니다.
        /// </summary>
        public bool MoveAlongMoveJob(CreatureMoveJob moveJob)
        {
            bool isArrived = MoveAlongPath(moveJob.Path, moveJob.PathIndex, out int nextPathIndex);
            moveJob.SetPathIndex(nextPathIndex);

            return isArrived;
        }

        private bool TryGetNodeWorldPosition(AStarNode node, out Vector3 worldPosition)
        {
            worldPosition = Vector3.zero;

            WorldGridController gridController = WorldManager.GetWorldGridController();
            if (gridController == null || gridController.Grid == null)
            {
                return false;
            }

            Vector3Int cellPosition = new Vector3Int(node.X, node.Y, 0);
            worldPosition = gridController.Grid.GetCellCenterWorld(cellPosition);
            return true;
        }
    }
}
