using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace TRPG.Runtime
{
    /// <summary>
    /// 여러 A* 요청을 병렬로 처리하는 Burst 컴파일 대상 Job입니다.
    /// </summary>
    [BurstCompile]
    public struct AStarJob : IJobParallelFor
    {
        /// <summary>
        /// 요청별 GCost 상태입니다. 요청마다 전체 노드 수만큼 독립된 영역을 사용합니다.
        /// </summary>
        public NativeArray<int> GCosts;

        /// <summary>
        /// 요청별 Parent 노드 index입니다. 경로를 역추적할 때 사용합니다.
        /// </summary>
        public NativeArray<int> Parents;

        /// <summary>
        /// 요청별 탐색 상태입니다. 0 = 미방문, 1 = Open, 2 = Closed.
        /// </summary>
        public NativeArray<byte> States;

        /// <summary>
        /// 요청별 OpenList입니다. 첫 구현은 BinaryHeap 대신 선형 검색을 사용합니다.
        /// </summary>
        public NativeArray<int> OpenList;

        /// <summary>
        /// 요청별 결과 경로 노드 index 목록입니다. 요청마다 MaxPathLength만큼 독립된 영역을 사용합니다.
        /// </summary>
        public NativeArray<int> PathNodeIndices;

        [Unity.Collections.ReadOnly] public NativeArray<byte> WalkableNodes;

        [Unity.Collections.ReadOnly] public NativeArray<AStarJobPathRequest> Requests;

        public NativeArray<AStarJobPathResult> Results;

        public int Width;
        public int Height;
        public int MaxPathLength;

        public void Execute(int index)
        {
            int nodeCount = Width * Height;
            int nodeOffset = index * nodeCount;
            int openOffset = index * nodeCount;
            int pathOffset = index * MaxPathLength;

            ResetPathState(nodeOffset, nodeCount);

            AStarJobPathRequest request = Requests[index];

            if (!Evaluate(request))
            {
                Results[index] = new AStarJobPathResult
                {
                    Length = 0,
                    Status = AStarJobPathStatus.InvalidRequest,
                };
                return;
            }

            // 요청의 시작 좌표와 목표 좌표를 1차원 노드 index로 변환한다.
            int startIndex = ToIndex(request.StartX, request.StartY);
            int targetIndex = ToIndex(request.TargetX, request.TargetY);

            // 현재 OpenList에 들어있는 노드 개수다.
            int openCount = 0;

            // 시작 노드는 시작점이므로 현재까지 이동 비용은 0이다.
            GCosts[nodeOffset + startIndex] = 0;

            // 시작 노드는 이전 노드가 없다.
            Parents[nodeOffset + startIndex] = -1;

            // 시작 노드를 Open 상태로 표시한다.
            // 0 = 미방문, 1 = Open, 2 = Closed
            States[nodeOffset + startIndex] = 1;

            // 시작 노드를 첫 번째 탐색 후보로 추가한다.
            OpenList[openOffset + openCount] = startIndex;
            openCount++;

            // OpenList가 비거나 목표 노드를 찾을 때까지 A* 탐색을 반복합니다.
            while (openCount > 0)
            {
                // OpenList 중 FCost가 가장 낮은 노드를 현재 노드로 선택합니다.
                int currentOpenIndex = GetLowestCostOpenIndex(openOffset, openCount, nodeOffset, targetIndex);
                int currentNodeIndex = OpenList[openOffset + currentOpenIndex];

                // 목표 노드에 도착했다면 Parent를 역추적해 최종 경로를 저장합니다.
                if (currentNodeIndex == targetIndex)
                {
                    BuildPath(index, nodeOffset, pathOffset, targetIndex);
                    return;
                }

                // 현재 노드는 검사가 끝났으므로 OpenList에서 제거하고 Closed 상태로 표시합니다.
                RemoveOpenAt(openOffset, ref openCount, currentOpenIndex);
                States[nodeOffset + currentNodeIndex] = 2;

                // 현재 노드의 1차원 index를 상하좌우 탐색에 사용할 x/y 좌표로 변환합니다.
                int currentX = ToX(currentNodeIndex);
                int currentY = ToY(currentNodeIndex);

                // 4방향 이웃 노드를 검사하고, 더 좋은 경로라면 OpenList에 반영합니다.
                TryOpenNeighbor(openOffset, nodeOffset, targetIndex, currentNodeIndex, currentX + 1, currentY, ref openCount);
                TryOpenNeighbor(openOffset, nodeOffset, targetIndex, currentNodeIndex, currentX - 1, currentY, ref openCount);
                TryOpenNeighbor(openOffset, nodeOffset, targetIndex, currentNodeIndex, currentX, currentY + 1, ref openCount);
                TryOpenNeighbor(openOffset, nodeOffset, targetIndex, currentNodeIndex, currentX, currentY - 1, ref openCount);
            }

            // OpenList가 비었다면 도달 가능한 노드를 모두 검사해도 경로가 없다는 뜻입니다.
            Results[index] = new AStarJobPathResult
            {
                Length = 0,
                Status = AStarJobPathStatus.NoPath,
            };
        }

        /// <summary>
        /// 2차원 좌표를 1차원 노드 index로 변환합니다.
        /// </summary>
        private int ToIndex(int x, int y)
        {
            return x + y * Width;
        }

        /// <summary>
        /// 1차원 노드 index에서 x 좌표를 계산합니다.
        /// </summary>
        private int ToX(int index)
        {
            return index % Width;
        }

        /// <summary>
        /// 1차원 노드 index에서 y 좌표를 계산합니다.
        /// </summary>
        private int ToY(int index)
        {
            return index / Width;
        }

        /// <summary>
        /// 좌표가 그리드 범위 안에 있는지 확인합니다.
        /// </summary>
        private bool IsInside(int x, int y)
        {
            return x >= 0 && x < Width
                && y >= 0 && y < Height;
        }

        /// <summary>
        /// 시작 좌표/목표 좌표가 맵 밖이거나, 못 걷는 칸이면 A*를 돌릴 필요가 없습니다.
        /// </summary>
        private bool Evaluate(AStarJobPathRequest request)
        {
            if (!IsInside(request.StartX, request.StartY))
            {
                return false;
            }

            if (!IsInside(request.TargetX, request.TargetY))
            {
                return false;
            }

            int startIndex = ToIndex(request.StartX, request.StartY);
            int targetIndex = ToIndex(request.TargetX, request.TargetY);

            if (WalkableNodes[startIndex] == 0)
            {
                return false;
            }

            if (WalkableNodes[targetIndex] == 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 현재 요청이 사용할 GCost, Parent, State 배열 영역을 초기화합니다.
        /// </summary>
        private void ResetPathState(int nodeOffset, int nodeCount)
        {
            for (int i = 0; i < nodeCount; i++)
            {
                // nodeOffset을 더해 요청별로 분리된 상태 배열 영역에 접근합니다.
                int stateIndex = nodeOffset + i;

                GCosts[stateIndex] = int.MaxValue;
                Parents[stateIndex] = -1;
                States[stateIndex] = 0;
            }
        }

        /// <summary>
        /// 노드의 FCost를 계산합니다. FCost는 현재까지 비용인 GCost와 목표까지 예상 비용인 HCost의 합입니다.
        /// </summary>
        private int CalculateFCost(int nodeOffset, int nodeIndex, int targetIndex)
        {
            int gCost = GCosts[nodeOffset + nodeIndex];
            int hCost = CalculateHeuristic(nodeIndex, targetIndex);

            return gCost + hCost;
        }

        /// <summary>
        /// OpenList에서 FCost가 가장 낮은 노드의 OpenList 위치를 찾습니다.
        /// </summary>
        private int GetLowestCostOpenIndex(int openOffset, int openCount, int nodeOffset, int targetIndex)
        {
            int bestOpenIndex = 0;
            int firstNodeIndex = OpenList[openOffset];
            int bestFCost = CalculateFCost(nodeOffset, firstNodeIndex, targetIndex);
            int bestHCost = CalculateHeuristic(firstNodeIndex, targetIndex);

            for (int i = 1; i < openCount; i++)
            {
                // 후보 노드의 FCost를 계산해 현재 best와 비교합니다.
                int candidateNodeIndex = OpenList[openOffset + i];
                int candidateFCost = CalculateFCost(nodeOffset, candidateNodeIndex, targetIndex);
                int candidateHCost = CalculateHeuristic(candidateNodeIndex, targetIndex);

                // FCost가 같다면 목표에 더 가까운 HCost가 낮은 노드를 우선합니다.
                if (candidateFCost < bestFCost ||
                    candidateFCost == bestFCost && candidateHCost < bestHCost)
                {
                    bestOpenIndex = i;
                    bestFCost = candidateFCost;
                    bestHCost = candidateHCost;
                }
            }

            return bestOpenIndex;
        }

        /// <summary>
        /// OpenList에서 특정 위치의 노드를 제거합니다.
        /// </summary>
        private void RemoveOpenAt(int openOffset, ref int openCount, int removeOpenIndex)
        {
            // 마지막 원소를 제거 위치로 덮어써서 O(1)로 제거합니다. OpenList 순서는 중요하지 않습니다.
            openCount--;
            OpenList[openOffset + removeOpenIndex] = OpenList[openOffset + openCount];
        }

        /// <summary>
        /// 현재 노드의 이웃 좌표를 검사하고 더 좋은 경로라면 상태 배열과 OpenList를 갱신합니다.
        /// </summary>
        private void TryOpenNeighbor(
            int openOffset,
            int nodeOffset,
            int targetIndex,
            int currentNodeIndex,
            int neighborX,
            int neighborY,
            ref int openCount)
        {
            // 맵 밖 좌표는 이웃으로 사용할 수 없습니다.
            if (!IsInside(neighborX, neighborY))
            {
                return;
            }

            int neighborIndex = ToIndex(neighborX, neighborY);
            int neighborStateIndex = nodeOffset + neighborIndex;

            // 이동 불가능한 노드이거나 이미 Closed 처리된 노드는 다시 검사하지 않습니다.
            if (WalkableNodes[neighborIndex] == 0 || States[neighborStateIndex] == 2)
            {
                return;
            }

            // 4방향 이동 비용은 현재 구현에서 1로 고정합니다.
            int newGCost = GCosts[nodeOffset + currentNodeIndex] + 1;
            if (newGCost >= GCosts[neighborStateIndex])
            {
                return;
            }

            // 더 저렴한 경로를 찾았으므로 비용과 Parent를 갱신합니다.
            GCosts[neighborStateIndex] = newGCost;
            Parents[neighborStateIndex] = currentNodeIndex;

            // 처음 방문한 노드라면 OpenList에 추가합니다.
            if (States[neighborStateIndex] == 0)
            {
                States[neighborStateIndex] = 1;
                OpenList[openOffset + openCount] = neighborIndex;
                openCount++;
            }
        }

        /// <summary>
        /// 목표 노드에서 Parent를 따라 시작 노드까지 역추적하고 결과 배열에 경로를 저장합니다.
        /// </summary>
        private void BuildPath(int requestIndex, int nodeOffset, int pathOffset, int targetIndex)
        {
            int length = 0;
            int currentIndex = targetIndex;

            // 먼저 경로 길이를 계산해서 MaxPathLength를 넘는지 확인합니다.
            while (currentIndex != -1)
            {
                length++;
                currentIndex = Parents[nodeOffset + currentIndex];

                if (length > MaxPathLength)
                {
                    Results[requestIndex] = new AStarJobPathResult
                    {
                        Length = 0,
                        Status = AStarJobPathStatus.PathTooLong,
                    };
                    return;
                }
            }

            // 목표에서 시작으로 역추적되므로, 결과 배열에는 뒤에서 앞으로 써서 시작 -> 목표 순서로 저장합니다.
            currentIndex = targetIndex;
            for (int writeIndex = length - 1; writeIndex >= 0; writeIndex--)
            {
                PathNodeIndices[pathOffset + writeIndex] = currentIndex;
                currentIndex = Parents[nodeOffset + currentIndex];
            }

            // 경로 저장이 끝났으므로 결과 상태를 성공으로 기록합니다.
            Results[requestIndex] = new AStarJobPathResult
            {
                Length = length,
                Status = AStarJobPathStatus.Success,
            };
        }

        /// <summary>
        /// 현재 노드에서 목표 노드까지의 예상 비용을 Manhattan Distance로 계산합니다.
        /// </summary>
        private int CalculateHeuristic(int currentIndex, int targetIndex)
        {
            int currentX = ToX(currentIndex);
            int currentY = ToY(currentIndex);

            int targetX = ToX(targetIndex);
            int targetY = ToY(targetIndex);

            int distanceX = Math.Abs(currentX - targetX);
            int distanceY = Math.Abs(currentY - targetY);

            return distanceX + distanceY;
        }
    }
}
