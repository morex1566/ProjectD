using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// AStarJob 실행 결과와 NativeArray 수명을 관리합니다.
    /// </summary>
    public sealed class AStarJobHandle : IDisposable
    {
        private readonly int width;
        private readonly int maxPathLength;
        private readonly NativeArray<AStarJobPathRequest> requests;
        private readonly NativeArray<AStarJobPathResult> results;
        private readonly NativeArray<int> pathNodeIndices;
        private readonly NativeArray<int> gCosts;
        private readonly NativeArray<int> parents;
        private readonly NativeArray<byte> states;
        private readonly NativeArray<int> openList;

        private JobHandle jobHandle;
        private bool isCompleted;
        private bool isDisposed;

        public int RequestCount => requests.Length;

        /// <summary>
        /// Job 실행에 사용한 NativeArray와 JobHandle을 보관합니다.
        /// </summary>
        public AStarJobHandle(
            int width,
            int maxPathLength,
            NativeArray<AStarJobPathRequest> requests,
            NativeArray<AStarJobPathResult> results,
            NativeArray<int> pathNodeIndices,
            NativeArray<int> gCosts,
            NativeArray<int> parents,
            NativeArray<byte> states,
            NativeArray<int> openList,
            JobHandle jobHandle)
        {
            this.width = width;
            this.maxPathLength = maxPathLength;
            this.requests = requests;
            this.results = results;
            this.pathNodeIndices = pathNodeIndices;
            this.gCosts = gCosts;
            this.parents = parents;
            this.states = states;
            this.openList = openList;
            this.jobHandle = jobHandle;
        }

        /// <summary>
        /// 스케줄된 AStarJob이 끝날 때까지 대기합니다.
        /// </summary>
        public void Complete()
        {
            if (isCompleted)
            {
                return;
            }

            // Job 결과 NativeArray를 읽기 전에 반드시 Complete 해야 합니다.
            jobHandle.Complete();
            isCompleted = true;
        }

        /// <summary>
        /// 특정 요청의 결과 상태를 반환합니다.
        /// </summary>
        public AStarJobPathResult GetResult(int requestIndex)
        {
            Complete();
            return results[requestIndex];
        }

        /// <summary>
        /// 모든 요청의 결과 경로를 Vector3Int 리스트로 변환합니다.
        /// </summary>
        public List<List<Vector3Int>> CompleteAndGetPaths()
        {
            Complete();

            // 요청 수와 같은 크기로 결과 리스트를 만듭니다.
            List<List<Vector3Int>> paths = new List<List<Vector3Int>>(RequestCount);
            for (int requestIndex = 0; requestIndex < RequestCount; requestIndex++)
            {
                AStarJobPathResult result = results[requestIndex];
                if (!result.IsSuccess)
                {
                    paths.Add(null);
                    continue;
                }

                paths.Add(BuildPath(requestIndex, result.Length));
            }

            return paths;
        }

        /// <summary>
        /// Job이 저장한 1차원 노드 index 경로를 2차원 셀 좌표 경로로 변환합니다.
        /// </summary>
        private List<Vector3Int> BuildPath(int requestIndex, int pathLength)
        {
            List<Vector3Int> path = new List<Vector3Int>(pathLength);
            int pathOffset = requestIndex * maxPathLength;

            for (int i = 0; i < pathLength; i++)
            {
                // Job은 경로를 노드 index로 저장하므로, 외부 사용을 위해 x/y 좌표로 되돌립니다.
                int nodeIndex = pathNodeIndices[pathOffset + i];
                int x = nodeIndex % width;
                int y = nodeIndex / width;
                path.Add(new Vector3Int(x, y, 0));
            }

            return path;
        }

        /// <summary>
        /// Job에서 사용한 NativeArray를 해제합니다.
        /// </summary>
        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            // 실행 중인 Job이 NativeArray를 사용 중일 수 있으므로 해제 전에 완료합니다.
            Complete();

            requests.Dispose();
            results.Dispose();
            pathNodeIndices.Dispose();
            gCosts.Dispose();
            parents.Dispose();
            states.Dispose();
            openList.Dispose();

            isDisposed = true;
        }
    }
}
