using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    // Creature 배회 목적지 선택 및 이동 로직
    public partial class CreatureController
    {
        /// <summary>
        /// 배회 Job을 한 틱 실행하고 완료 여부를 반환합니다.
        /// </summary>
        public bool TryExecuteWanderJob(CreatureWanderJob wanderJob, out bool isCompleted)
        {
            isCompleted = false;

            if (wanderJob.HasStarted == false)
            {
                wanderJob.Begin(Time.time + wanderJob.StartDelaySec);
            }

            if (Time.time < wanderJob.StartTime)
            {
                return true;
            }

            if (wanderJob.PathWorldPositions.Count > 1 && wanderJob.PathIndex < wanderJob.PathWorldPositions.Count)
            {
                isCompleted = MoveAlongWorldPath(wanderJob.PathWorldPositions, wanderJob.PathIndex, out int nextPathIndex);
                wanderJob.SetPathIndex(nextPathIndex);
                return true;
            }

            if (TryPickWanderPath(wanderJob) == false)
            {
                isCompleted = true;
                return false;
            }

            isCompleted = wanderJob.PathWorldPositions.Count <= 1;
            return true;
        }

        /// <summary>
        /// 현재 Creature 위치 기준으로 도달 가능한 배회 경로를 선택합니다.
        /// </summary>
        public bool TryPickWanderPath(CreatureWanderJob wanderJob)
        {
            List<Vector3Int> candidateCellPositions = new();
            Vector3Int currentCellPosition = WorldManager.WorldToCell(transform.position);

            for (int x = -wanderJob.WanderRadius; x <= wanderJob.WanderRadius; x++)
            {
                if (x == 0)
                {
                    continue;
                }

                Vector3Int candidateCellPosition = currentCellPosition + new Vector3Int(x, 0, 0);
                if (context.AIType == AIType.Ground && IsGroundReachableCell(candidateCellPosition) == false)
                {
                    continue;
                }

                candidateCellPositions.Add(candidateCellPosition);
            }

            Shuffle(candidateCellPositions);

            for (int i = 0; i < candidateCellPositions.Count; i++)
            {
                Vector3Int targetCellPosition = candidateCellPositions[i];
                List<AStarNode> path = AStarPathfinder.FindPath(currentCellPosition, targetCellPosition);
                if (path == null || path.Count <= 0)
                {
                    continue;
                }

                if (TryCreateWorldPath(path, useRandomOffset: true, out List<Vector3> pathWorldPositions) == false)
                {
                    return false;
                }

                wanderJob.SetPath(targetCellPosition, pathWorldPositions);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 대상 칸이 지상 Creature가 설 수 있는 칸인지 확인합니다.
        /// </summary>
        private bool IsGroundReachableCell(Vector3Int cellPosition)
        {
            if (AStarPathfinder.AStarGrid.TryGetNode(cellPosition.x, cellPosition.y, out _) == false)
            {
                return false;
            }

            WorldTilemapController groundTilemap = WorldManager.GetWorldTilemapController(WorldTilemapType.WorldTilemapGround);
            if (groundTilemap == null)
            {
                return false;
            }

            Vector3Int belowCellPosition = cellPosition + Vector3Int.down;
            return groundTilemap.TryGetTile(belowCellPosition.x, belowCellPosition.y, out _);
        }

        /// <summary>
        /// AStar 경로를 실제 이동에 사용할 월드 좌표 경로로 변환합니다.
        /// </summary>
        private bool TryCreateWorldPath(IReadOnlyList<AStarNode> path, bool useRandomOffset, out List<Vector3> pathWorldPositions)
        {
            pathWorldPositions = new List<Vector3>();

            WorldGridController gridController = WorldManager.GetWorldGridController();
            if (gridController == null || gridController.Grid == null)
            {
                return false;
            }

            for (int i = 0; i < path.Count; i++)
            {
                AStarNode node = path[i];
                Vector3Int cellPosition = new Vector3Int(node.X, node.Y, 0);
                Vector3 worldPosition = gridController.Grid.GetCellCenterWorld(cellPosition);

                if (useRandomOffset == true)
                {
                    worldPosition += AStarPathfinder.RandomOffset;
                }

                pathWorldPositions.Add(worldPosition);
            }

            return true;
        }

        /// <summary>
        /// 후보 배회 칸 순서를 섞습니다.
        /// </summary>
        private void Shuffle<T>(IList<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int randomIndex = Random.Range(i, list.Count);

                T temp = list[i];
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }
    }
}
