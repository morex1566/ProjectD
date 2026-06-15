using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace TRPG.Runtime
{
    public class AStarPathfinder : MonoBehaviour
    {
        private static AStarGrid astarGrid;
        private static NativeArray<byte> walkableNodes;

        private void Start()
        {
            var mapGenerater = GetComponent<Map>();
            astarGrid = new AStarGrid(mapGenerater.MapWidth, mapGenerater.MapHeight);

            RefreshJobGrid();
        }

        private void OnDestroy()
        {
            DisposeJobGrid();
        }

        /// <summary>
        /// 길찾기 시작
        /// </summary>
        public static List<AStarNode> FindPath(Vector3Int startPos, Vector3Int targetPos)
        {
            if (astarGrid == null)
            {
                return null;
            }

            AStarNode startNode = null;
            if (!astarGrid.TryGetNode(startPos.x, startPos.y, out startNode))
            {
                return null;
            }

            AStarNode targetNode = null;
            if (!astarGrid.TryGetNode(targetPos.x, targetPos.y, out targetNode))
            {
                return null;
            }

            return astarGrid.FindPath(startNode, targetNode);
        }

        /// <summary>
        /// 특정 셀의 이동 가능 여부를 갱신합니다.
        /// </summary>
        public static void SetWalkable(Vector3Int cellPos, bool isWalkable)
        {
            if (astarGrid == null || !astarGrid.IsInside(cellPos.x, cellPos.y))
            {
                return;
            }

            // 기존 동기 A* 그리드 상태를 먼저 갱신합니다.
            astarGrid.SetWalkable(cellPos.x, cellPos.y, isWalkable);

            // Job용 1차원 Walkable 배열도 같은 좌표를 갱신합니다.
            if (walkableNodes.IsCreated)
            {
                int index = astarGrid.ToIndex(cellPos.x, cellPos.y);
                walkableNodes[index] = isWalkable ? (byte)1 : (byte)0;
            }
        }

        /// <summary>
        /// 여러 A* 경로 요청을 Job System으로 병렬 스케줄합니다.
        /// </summary>
        public static AStarJobHandle ScheduleFindPaths(
            IReadOnlyList<Vector3Int> startPositions,
            IReadOnlyList<Vector3Int> targetPositions,
            int maxPathLength = 256,
            int innerloopBatchCount = 32,
            Allocator allocator = Allocator.TempJob)
        {
            if (astarGrid == null)
            {
                return null;
            }

            if (startPositions == null || targetPositions == null || startPositions.Count != targetPositions.Count)
            {
                return null;
            }

            if (startPositions.Count == 0)
            {
                return null;
            }

            // Job이 읽을 Walkable 배열이 없으면 현재 그리드 상태로 새로 만듭니다.
            if (!walkableNodes.IsCreated)
            {
                RefreshJobGrid();
            }

            maxPathLength = Mathf.Max(1, maxPathLength);
            innerloopBatchCount = Mathf.Max(1, innerloopBatchCount);

            int requestCount = startPositions.Count;
            int nodeCount = astarGrid.NodeCount;

            NativeArray<AStarJobPathRequest> requests = new NativeArray<AStarJobPathRequest>(requestCount, allocator);
            NativeArray<AStarJobPathResult> results = new NativeArray<AStarJobPathResult>(requestCount, allocator);
            NativeArray<int> pathNodeIndices = new NativeArray<int>(requestCount * maxPathLength, allocator);

            // 각 요청은 독립적으로 탐색하므로 요청마다 전체 노드 수만큼 상태 영역을 분리합니다.
            NativeArray<int> gCosts = new NativeArray<int>(requestCount * nodeCount, allocator);
            NativeArray<int> parents = new NativeArray<int>(requestCount * nodeCount, allocator);
            NativeArray<byte> states = new NativeArray<byte>(requestCount * nodeCount, allocator);
            NativeArray<int> openList = new NativeArray<int>(requestCount * nodeCount, allocator);

            // 외부 입력 좌표를 Job 요청 구조체 배열로 복사합니다.
            for (int i = 0; i < requestCount; i++)
            {
                Vector3Int startPos = startPositions[i];
                Vector3Int targetPos = targetPositions[i];
                requests[i] = new AStarJobPathRequest(startPos.x, startPos.y, targetPos.x, targetPos.y);
            }

            AStarJob job = new AStarJob
            {
                GCosts = gCosts,
                Parents = parents,
                States = states,
                OpenList = openList,
                PathNodeIndices = pathNodeIndices,
                WalkableNodes = walkableNodes,
                Requests = requests,
                Results = results,
                Width = astarGrid.Width,
                Height = astarGrid.Height,
                MaxPathLength = maxPathLength,
            };

            // 요청 하나당 Execute(index)가 한 번 실행됩니다.
            JobHandle jobHandle = job.Schedule(requestCount, innerloopBatchCount);
            return new AStarJobHandle(
                astarGrid.Width,
                maxPathLength,
                requests,
                results,
                pathNodeIndices,
                gCosts,
                parents,
                states,
                openList,
                jobHandle);
        }

        /// <summary>
        /// 현재 AStarGrid의 Walkable 상태를 Job용 NativeArray에 복사합니다.
        /// </summary>
        public static void RefreshJobGrid()
        {
            if (astarGrid == null)
            {
                return;
            }

            // 맵 크기가 바뀌었거나 아직 생성되지 않았다면 Persistent 배열을 새로 확보합니다.
            if (!walkableNodes.IsCreated || walkableNodes.Length != astarGrid.NodeCount)
            {
                DisposeJobGrid();
                walkableNodes = new NativeArray<byte>(astarGrid.NodeCount, Allocator.Persistent);
            }

            astarGrid.CopyWalkableTo(walkableNodes);
        }

        /// <summary>
        /// Job용 Persistent NativeArray를 해제합니다.
        /// </summary>
        private static void DisposeJobGrid()
        {
            if (walkableNodes.IsCreated)
            {
                walkableNodes.Dispose();
            }
        }
    }
}
