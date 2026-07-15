using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// 월드 타일을 기반으로 플랫폼 이동 경로를 탐색합니다.
    /// </summary>
    public static class WorldPathfinder
    {
        private static readonly Vector2Int[] walkDirections =
        {
            Vector2Int.left,
            Vector2Int.right,
        };

        /// <summary>
        /// 걷기만 사용하여 시작 타일부터 목표 타일까지 경로를 탐색합니다.
        /// </summary>
        public static bool TryFindWalkPath(WorldMap worldMap, Vector2Int startCoordinate, Vector2Int goalCoordinate, out List<WorldPathAction> path)
        {
            path = new List<WorldPathAction>();

            // 이동할 수 있는 위치?
            if (IsStandable(worldMap, startCoordinate) == false ||
                IsStandable(worldMap, goalCoordinate) == false)
            {
                return false;
            }

            // 제자리?
            if (startCoordinate == goalCoordinate)
            {
                return true;
            }

            List<Vector2Int> openCoordinates = new();
            HashSet<Vector2Int> closedCoordinates = new();
            Dictionary<Vector2Int, int> costsFromStart = new();
            Dictionary<Vector2Int, Vector2Int> previousCoordinates = new();
            Dictionary<Vector2Int, WorldPathAction> previousActions = new();

            openCoordinates.Add(startCoordinate);
            costsFromStart.Add(startCoordinate, 0);

            while (openCoordinates.Count > 0)
            {
                // 예상 후보군에서 총비용이 가장 낮은 좌표
                Vector2Int currentCoordinate = GetLowestCostCoordinate(openCoordinates, costsFromStart, goalCoordinate);
                openCoordinates.Remove(currentCoordinate);

                // 도착했음?
                if (currentCoordinate == goalCoordinate)
                {
                    path = ReconstructPath(startCoordinate, goalCoordinate, previousCoordinates, previousActions);
                    return true;
                }

                // 일단 계산 완료 처리
                closedCoordinates.Add(currentCoordinate);

                // 주변으로 이제 이동 검사 시작
                foreach (Vector2Int direction in walkDirections)
                {
                    Vector2Int neighborCoordinate = currentCoordinate + direction;

                    // 이미 검사된 곳?
                    if (closedCoordinates.Contains(neighborCoordinate))
                    {
                        continue;
                    }

                    // 이동 가능하긴 함?
                    if (IsStandable(worldMap, neighborCoordinate) == false)
                    {
                        continue;
                    }

                    // 저렴한 이동으로 선택
                    int newCost = costsFromStart[currentCoordinate] + 1;
                    if (costsFromStart.TryGetValue(neighborCoordinate, out int previousCost) &&
                        newCost >= previousCost)
                    {
                        continue;
                    }
                    else
                    {
                        costsFromStart[neighborCoordinate] = newCost;
                        previousCoordinates[neighborCoordinate] = currentCoordinate;
                        previousActions[neighborCoordinate] = new WorldPathAction(
                            WorldPathActionType.Walk,
                            currentCoordinate,
                            neighborCoordinate,
                            1);
                    }

                    // 아직 탐색 예정 목록에 없다면 추가
                    if (openCoordinates.Contains(neighborCoordinate) == false)
                    {
                        openCoordinates.Add(neighborCoordinate);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 지정한 타일에 캐릭터가 설 수 있는지 확인합니다.
        /// </summary>
        public static bool IsStandable(WorldMap worldMap, Vector2Int coordinate)
        {
            if (coordinate.x < 0 || coordinate.y <= 0)
            {
                return false;
            }

            if (worldMap.TryGetTile(coordinate, out WorldTile currentTile) == false)
            {
                return false;
            }

            if (currentTile.IsEmpty == false)
            {
                return false;
            }

            Vector2Int groundCoordinate = coordinate + Vector2Int.down;

            if (worldMap.TryGetTile(groundCoordinate, out WorldTile groundTile) == false)
            {
                return false;
            }

            return groundTile.IsEmpty == false;
        }

        /// <summary>
        /// Open 목록에서 예상 총비용이 가장 낮은 좌표를 반환합니다.
        /// </summary>
        private static Vector2Int GetLowestCostCoordinate(List<Vector2Int> openCoordinates, Dictionary<Vector2Int, int> costsFromStart, Vector2Int goalCoordinate)
        {
            Vector2Int bestCoordinate = openCoordinates[0];
            int bestHeuristic = GetHeuristic(bestCoordinate, goalCoordinate);
            int bestTotalCost = costsFromStart[bestCoordinate] + bestHeuristic;

            for (int index = 1; index < openCoordinates.Count; index++)
            {
                Vector2Int coordinate = openCoordinates[index];
                int heuristic = GetHeuristic(coordinate, goalCoordinate);
                int totalCost = costsFromStart[coordinate] + heuristic;

                if (totalCost > bestTotalCost)
                {
                    continue;
                }

                if (totalCost == bestTotalCost && heuristic >= bestHeuristic)
                {
                    continue;
                }

                bestCoordinate = coordinate;
                bestHeuristic = heuristic;
                bestTotalCost = totalCost;
            }

            return bestCoordinate;
        }

        /// <summary>
        /// 현재 좌표에서 목표 좌표까지의 낙관적인 이동 비용을 계산합니다.
        /// </summary>
        private static int GetHeuristic(Vector2Int currentCoordinate, Vector2Int goalCoordinate)
        {
            return Mathf.Abs(goalCoordinate.x - currentCoordinate.x);
        }

        /// <summary>
        /// 목표에서 시작 방향으로 저장된 행동을 올바른 실행 순서로 복원합니다.
        /// </summary>
        private static List<WorldPathAction> ReconstructPath(
            Vector2Int startCoordinate,
            Vector2Int goalCoordinate,
            Dictionary<Vector2Int, Vector2Int> previousCoordinates,
            Dictionary<Vector2Int, WorldPathAction> previousActions)
        {
            List<WorldPathAction> reversedPath = new List<WorldPathAction>();
            Vector2Int currentCoordinate = goalCoordinate;

            while (currentCoordinate != startCoordinate)
            {
                reversedPath.Add(previousActions[currentCoordinate]);
                currentCoordinate = previousCoordinates[currentCoordinate];
            }

            reversedPath.Reverse();

            return reversedPath;
        }
    }
}
