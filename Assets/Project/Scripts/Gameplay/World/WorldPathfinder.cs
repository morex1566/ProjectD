using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 월드 타일을 기반으로 플랫폼 이동 경로를 탐색합니다.
    /// </summary>
    public static class WorldPathfinder
    {
        private static readonly Vector2Int[] walkDirections =
        {
            Vector2Int.left,
            Vector2Int.right,
        };

        private static readonly Vector2Int[] fallDirections =
        {
            Vector2Int.left,
            Vector2Int.right,
        };

        /// <summary>
        /// 걷기만 사용하여 시작 타일부터 목표 타일까지 경로를 탐색합니다.
        /// </summary>
        public static bool TryFindPath(WorldMap worldMap, Vector2Int startCoordinate, Vector2Int goalCoordinate, WorldPathMovementProfile movementProfile, out List<WorldPathAction> path)
        {
            path = new List<WorldPathAction>();

            if (movementProfile.IsValid() == false)
            {
                return false;
            }

            // 시작과 목표에 크리처 몸체 전체가 들어갈 수 있어야 합니다.
            if (IsStandable(worldMap, startCoordinate, movementProfile.BodyHeight) == false ||
                IsStandable(worldMap, goalCoordinate, movementProfile.BodyHeight) == false)
            {
                return false;
            }

            if (startCoordinate == goalCoordinate)
            {
                return true;
            }

            List<Vector2Int> openCoordinates = new();
            HashSet<Vector2Int> closedCoordinates = new();
            Dictionary<Vector2Int, int> costsFromStart = new();
            Dictionary<Vector2Int, Vector2Int> previousCoordinates = new();
            Dictionary<Vector2Int, WorldPathAction> previousActions = new();
            List<WorldPathAction> availableActions = new();

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
                availableActions.Clear();

                // 이제 이동검사
                AddAvailableActions(worldMap, currentCoordinate, movementProfile, availableActions);
                foreach (WorldPathAction action in availableActions)
                {
                    Vector2Int neighborCoordinate = action.To;

                    if (closedCoordinates.Contains(neighborCoordinate) == true)
                    {
                        continue;
                    }

                    int newCost = costsFromStart[currentCoordinate] + action.Cost;

                    if (costsFromStart.TryGetValue(neighborCoordinate, out int previousCost) == true &&
                        newCost >= previousCost)
                    {
                        continue;
                    }

                    costsFromStart[neighborCoordinate] = newCost;
                    previousCoordinates[neighborCoordinate] = currentCoordinate;
                    previousActions[neighborCoordinate] = action;

                    if (openCoordinates.Contains(neighborCoordinate) == false)
                    {
                        openCoordinates.Add(neighborCoordinate);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 현재 좌표에서 실행 가능한 모든 플랫폼 행동을 추가합니다.
        /// 현재 위치에서 실행 가능한 Walk, CalculateJumpPosition, Fall 행동의 도착점이 이웃 노드가 됩니다.
        /// </summary>
        private static void AddAvailableActions(
            WorldMap worldMap,
            Vector2Int currentCoordinate,
            WorldPathMovementProfile movementProfile,
            List<WorldPathAction> actions)
        {
            AddWalkActions(worldMap, currentCoordinate, movementProfile, actions);
            AddJumpActions(worldMap, currentCoordinate, movementProfile, actions);
            AddFallActions(worldMap, currentCoordinate, movementProfile, actions);
        }

        /// <summary>
        /// 현재 좌표에서 좌우로 걸을 수 있는 행동을 추가합니다.
        /// </summary>
        private static void AddWalkActions(
            WorldMap worldMap,
            Vector2Int currentCoordinate,
            WorldPathMovementProfile movementProfile,
            List<WorldPathAction> actions)
        {
            foreach (Vector2Int direction in walkDirections)
            {
                Vector2Int targetCoordinate = currentCoordinate + direction;

                if (IsStandable(worldMap, targetCoordinate, movementProfile.BodyHeight) == false)
                {
                    continue;
                }

                int cost = 1;

                // TODO : 코스트 선정에서 좀 신경을 써야할듯
                actions.Add(new WorldPathAction(
                    WorldPathActionType.Walk,
                    currentCoordinate,
                    targetCoordinate,
                    cost));
            }
        }

        /// <summary>
        /// 현재 위치에서 같은 높이 또는 높은 발판으로 점프할 수 있는 행동을 추가합니다.
        /// </summary>
        private static void AddJumpActions(
            WorldMap worldMap,
            Vector2Int startCoordinate,
            WorldPathMovementProfile movementProfile,
            List<WorldPathAction> actions)
        {
            int maximumHorizontalDistance = movementProfile.MaximumJumpHorizontalDistance;
            int maximumVerticalDistance = movementProfile.MaximumJumpHeight;

            for (int verticalDistance = 0; verticalDistance <= maximumVerticalDistance; verticalDistance++)
            {
                for (int horizontalDistance = -maximumHorizontalDistance; horizontalDistance <= maximumHorizontalDistance; horizontalDistance++)
                {
                    if (horizontalDistance == 0)
                    {
                        continue;
                    }

                    Vector2Int targetCoordinate = startCoordinate + new Vector2Int(horizontalDistance, verticalDistance);

                    if (IsStandable(worldMap, targetCoordinate, movementProfile.BodyHeight) == false)
                    {
                        continue;
                    }

                    if (IsJumpTrajectoryClear(worldMap, startCoordinate, targetCoordinate, movementProfile.BodyHeight) == false)
                    {
                        continue;
                    }

                    int cost = Mathf.Abs(horizontalDistance) + verticalDistance + 2;

                    // TODO : 코스트 선정에서 좀 신경을 써야할듯
                    actions.Add(new WorldPathAction(
                        WorldPathActionType.Jump,
                        startCoordinate,
                        targetCoordinate,
                        cost));
                }
            }
        }

        /// <summary>
        /// 현재 발판의 좌우 끝에서 아래 발판으로 낙하하는 행동을 추가합니다.
        /// </summary>
        private static void AddFallActions(
            WorldMap worldMap,
            Vector2Int startCoordinate,
            WorldPathMovementProfile movementProfile,
            List<WorldPathAction> actions)
        {
            foreach (Vector2Int direction in fallDirections)
            {
                Vector2Int fallEntryCoordinate = startCoordinate + direction;

                // 발판 밖으로 나가는 첫 위치에 몸체가 들어갈 수 있어야 합니다.
                if (IsBodyAreaEmpty(worldMap, fallEntryCoordinate, movementProfile.BodyHeight) == false)
                {
                    continue;
                }

                // 옆 타일에 바로 설 수 있다면 낙하가 아니라 Walk입니다.
                if (IsStandable(worldMap, fallEntryCoordinate, movementProfile.BodyHeight) == true)
                {
                    continue;
                }

                if (TryFindFallLanding(
                    worldMap,
                    fallEntryCoordinate,
                    movementProfile,
                    out Vector2Int landingCoordinate,
                    out int fallDistance) == false)
                {
                    continue;
                }

                int cost = fallDistance + 2;

                actions.Add(new WorldPathAction(
                    WorldPathActionType.Fall,
                    startCoordinate,
                    landingCoordinate,
                    cost));
            }
        }

        /// <summary>
        /// 낙하 진입 좌표 아래에서 처음 만나는 유효한 착지점을 찾습니다.
        /// </summary>
        private static bool TryFindFallLanding(
            WorldMap worldMap,
            Vector2Int fallEntryCoordinate,
            WorldPathMovementProfile movementProfile,
            out Vector2Int landingCoordinate,
            out int fallDistance)
        {
            landingCoordinate = default;
            fallDistance = 0;

            for (int distance = 1; distance <= movementProfile.MaximumFallDistance; distance++)
            {
                Vector2Int candidateCoordinate =
                    fallEntryCoordinate + Vector2Int.down * distance;

                if (candidateCoordinate.y <= 0)
                {
                    return false;
                }

                // 몸체가 지형에 부딪혔다면 그 아래로 통과할 수 없습니다.
                if (IsBodyAreaEmpty(
                    worldMap,
                    candidateCoordinate,
                    movementProfile.BodyHeight) == false)
                {
                    return false;
                }

                if (IsStandable(
                    worldMap,
                    candidateCoordinate,
                    movementProfile.BodyHeight) == false)
                {
                    continue;
                }

                landingCoordinate = candidateCoordinate;
                fallDistance = distance;

                return true;
            }

            return false;
        }

        /// <summary>
        /// 점프 곡선 전체에서 크리처 몸체가 지형과 충돌하지 않는지 확인합니다.
        /// </summary>
        private static bool IsJumpTrajectoryClear(
            WorldMap worldMap,
            Vector2Int startCoordinate,
            Vector2Int targetCoordinate,
            int bodyHeight)
        {
            float middleHeight = (startCoordinate.y + targetCoordinate.y) * 0.5f;
            float apexHeight = Mathf.Max(startCoordinate.y, targetCoordinate.y) + 1f;
            float arcHeight = apexHeight - middleHeight;

            int horizontalDistance = Mathf.Abs(targetCoordinate.x - startCoordinate.x);
            int verticalDistance = Mathf.CeilToInt(apexHeight - Mathf.Min(startCoordinate.y, targetCoordinate.y));
            int sampleCount = Mathf.Max(4, Mathf.Max(horizontalDistance, verticalDistance) * 4);

            for (int sampleIndex = 1; sampleIndex <= sampleCount; sampleIndex++)
            {
                float ratio = sampleIndex / (float)sampleCount;

                Vector2 position;

                if (sampleIndex == sampleCount)
                {
                    // 부동소수점 오차로 착지 위치가 아래 타일로 계산되지 않도록 정확한 목표 좌표를 사용합니다.
                    position = targetCoordinate;
                }
                else
                {
                    float horizontalPosition = Mathf.Lerp(startCoordinate.x, targetCoordinate.x, ratio);
                    float linearVerticalPosition = Mathf.Lerp(startCoordinate.y, targetCoordinate.y, ratio);
                    float arcOffset = 4f * arcHeight * ratio * (1f - ratio);
                    float verticalPosition = linearVerticalPosition + arcOffset;

                    position = new Vector2(horizontalPosition, verticalPosition);
                }

                if (IsBodyAreaEmpty(worldMap, position, bodyHeight) == false)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 요청 좌표와 같은 열에서 가장 가까운 한 칸짜리 캐릭터 이동 좌표를 찾습니다.
        /// 같은 거리에서는 위쪽 좌표를 우선합니다.
        /// </summary>
        public static bool TryFindNearestStandableCoordinate(
            WorldMap worldMap,
            Vector2Int requestedCoordinate,
            int maximumVerticalSearchDistance,
            out Vector2Int standableCoordinate)
        {
            standableCoordinate = default;

            if (worldMap == null || maximumVerticalSearchDistance < 0)
            {
                return false;
            }

            if (IsStandable(worldMap, requestedCoordinate) == true)
            {
                standableCoordinate = requestedCoordinate;
                return true;
            }

            for (int distance = 1; distance <= maximumVerticalSearchDistance; distance++)
            {
                Vector2Int upperCoordinate = requestedCoordinate + Vector2Int.up * distance;
                if (IsStandable(worldMap, upperCoordinate) == true)
                {
                    standableCoordinate = upperCoordinate;
                    return true;
                }

                Vector2Int lowerCoordinate = requestedCoordinate + Vector2Int.down * distance;
                if (IsStandable(worldMap, lowerCoordinate) == true)
                {
                    standableCoordinate = lowerCoordinate;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 연속 좌표에 가로 1타일, 지정한 높이의 몸체를 배치할 수 있는지 확인합니다.
        /// </summary>
        private static bool IsBodyAreaEmpty(WorldMap worldMap, Vector2 bottomLeftPosition, int bodyHeight)
        {
            const float boundaryEpsilon = 0.001f;

            int minimumX = Mathf.FloorToInt(bottomLeftPosition.x);
            int minimumY = Mathf.FloorToInt(bottomLeftPosition.y);
            int maximumX = Mathf.CeilToInt(bottomLeftPosition.x + 1f - boundaryEpsilon) - 1;
            int maximumY = Mathf.CeilToInt(bottomLeftPosition.y + bodyHeight - boundaryEpsilon) - 1;

            for (int y = minimumY; y <= maximumY; y++)
            {
                for (int x = minimumX; x <= maximumX; x++)
                {
                    if (IsEmptyTile(worldMap, new Vector2Int(x, y)) == false)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 지정한 월드 좌표가 실제로 존재하는 빈 타일인지 확인합니다.
        /// </summary>
        private static bool IsEmptyTile(WorldMap worldMap, Vector2Int coordinate)
        {
            if (coordinate.x < 0 || coordinate.y < 0)
            {
                return false;
            }

            if (worldMap.TryGetTile(coordinate, out WorldTile tile) == false)
            {
                return false;
            }

            return tile.IsEmpty;
        }

        public static bool IsStandable(WorldMap worldMap, Vector2Int coordinate)
        {
            const int navigationBodyHeight = 1;
            return IsStandable(worldMap, coordinate, navigationBodyHeight);
        }

        /// <summary>
        /// 지정한 발 위치에 주어진 높이의 크리처가 설 수 있는지 확인합니다.
        /// </summary>
        public static bool IsStandable(WorldMap worldMap, Vector2Int coordinate, int bodyHeight)
        {
            if (coordinate.x < 0 || coordinate.y <= 0 || bodyHeight <= 0)
            {
                return false;
            }

            // 발부터 머리까지 크리처가 차지하는 모든 타일이 비어 있어야 합니다.
            for (int heightOffset = 0; heightOffset < bodyHeight; heightOffset++)
            {
                Vector2Int bodyCoordinate = coordinate + Vector2Int.up * heightOffset;

                if (worldMap.TryGetTile(bodyCoordinate, out WorldTile bodyTile) == false)
                {
                    return false;
                }

                if (bodyTile.IsEmpty == false)
                {
                    return false;
                }
            }

            // 발 바로 아래에는 크리처를 지지하는 지형이 있어야 합니다.
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
