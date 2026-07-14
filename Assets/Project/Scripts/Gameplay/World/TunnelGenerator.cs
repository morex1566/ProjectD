using System;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// cave 공간 사이를 이어주는 터널을 생성
    /// </summary>
    [Serializable]
    public sealed class TunnelGenerator
    {
        private static readonly Vector2Int[] neighborDirections =
        {
            Vector2Int.left,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.up,
        };

        [SerializeField] private bool isEnabled = true;

        [SerializeField, Min(1)] private int radius = 2;

        [SerializeField, Range(0f, 0.5f)] private float curveOffsetRatio = 0.2f;


        /// <summary>
        /// 월드의 모든 동굴 영역을 하나의 연결망으로 만듭니다.
        /// </summary>
        public void Generate(WorldMap worldMap, int seed)
        {
            if (isEnabled == false)
            {
                return;
            }

            List<List<Vector2Int>> caveRegions = WorldPathfinder.FindCaveRegions(worldMap);
            if (caveRegions.Count <= 1)
            {
                return;
            }

            // 각 리전의 외곽좌표들만 모아둔다
            List<List<Vector2Int>> regionBoundaries = GetRegionBoundaries(worldMap, caveRegions);
            int largestRegionIndex = FindLargestRegionIndex(caveRegions);
            HashSet<int> connectedRegionIndexes = new HashSet<int>();
            {
                connectedRegionIndexes.Add(largestRegionIndex);
            }

            // 이 외곽 좌표들을 이용해서 각각 동굴을 연결
            while (connectedRegionIndexes.Count < caveRegions.Count)
            {
                FindClosestConnection(
                    regionBoundaries,
                    connectedRegionIndexes,
                    out Vector2Int startCoordinate,
                    out Vector2Int endCoordinate,
                    out int endRegionIndex);

                System.Random random = new System.Random(seed);
                CarveTunnel(worldMap, startCoordinate, endCoordinate, random);

                connectedRegionIndexes.Add(endRegionIndex);
            }
        }

        /// <summary>
        /// 각 동굴 영역을 대표하는 중심 타일을 계산합니다.
        /// </summary>
        private static List<Vector2Int> GetRegionCenters(List<List<Vector2Int>> caveRegions)
        {
            List<Vector2Int> regionCenters = new List<Vector2Int>();

            foreach (List<Vector2Int> region in caveRegions)
            {
                regionCenters.Add(GetRegionCenter(region));
            }

            return regionCenters;
        }

        /// <summary>
        /// 동굴의 평균 좌표와 가장 가까운 실제 Empty 타일을 반환합니다.
        /// </summary>
        private static Vector2Int GetRegionCenter(List<Vector2Int> region)
        {
            Vector2 averagePosition = Vector2.zero;

            foreach (Vector2Int coordinate in region)
            {
                averagePosition += coordinate;
            }

            averagePosition /= region.Count;

            Vector2Int centerCoordinate = region[0];
            float closestDistance = ((Vector2)centerCoordinate - averagePosition).sqrMagnitude;

            foreach (Vector2Int coordinate in region)
            {
                float distance = ((Vector2)coordinate - averagePosition).sqrMagnitude;

                if (distance < closestDistance)
                {
                    centerCoordinate = coordinate;
                    closestDistance = distance;
                }
            }

            return centerCoordinate;
        }

        /// <summary>
        /// 각 동굴 영역에서 고체 타일과 맞닿은 경계 타일을 찾습니다.
        /// </summary>
        private static List<List<Vector2Int>> GetRegionBoundaries(WorldMap worldMap, List<List<Vector2Int>> caveRegions)
        {
            List<List<Vector2Int>> regionBoundaries = new List<List<Vector2Int>>();

            foreach (List<Vector2Int> region in caveRegions)
            {
                regionBoundaries.Add(GetRegionBoundary(worldMap, region));
            }

            return regionBoundaries;
        }

        /// <summary>
        /// 하나의 동굴에서 Stone과 맞닿은 Empty 타일을 반환합니다.
        /// </summary>
        private static List<Vector2Int> GetRegionBoundary(WorldMap worldMap, List<Vector2Int> region)
        {
            List<Vector2Int> boundary = new List<Vector2Int>();

            foreach (Vector2Int coordinate in region)
            {
                foreach (Vector2Int direction in neighborDirections)
                {
                    Vector2Int neighborCoordinate = coordinate + direction;

                    if (worldMap.TryGetTile(neighborCoordinate, out WorldTile neighborTile) == false)
                    {
                        continue;
                    }

                    if (neighborTile.IsEmpty)
                    {
                        continue;
                    }

                    boundary.Add(coordinate);
                    break;
                }
            }

            return boundary;
        }

        /// <summary>
        /// 가장 많은 Empty 타일을 가진 동굴의 인덱스를 반환합니다.
        /// </summary>
        private static int FindLargestRegionIndex(List<List<Vector2Int>> caveRegions)
        {
            int largestRegionIndex = 0;

            for (int regionIndex = 1; regionIndex < caveRegions.Count; regionIndex++)
            {
                if (caveRegions[regionIndex].Count > caveRegions[largestRegionIndex].Count)
                {
                    largestRegionIndex = regionIndex;
                }
            }

            return largestRegionIndex;
        }

        /// <summary>
        /// 연결된 동굴과 미연결 동굴 사이에서 가장 가까운 경계 타일 쌍을 찾습니다.
        /// </summary>
        private static void FindClosestConnection(
            List<List<Vector2Int>> regionBoundaries,
            HashSet<int> connectedRegionIndexes,
            out Vector2Int startCoordinate,
            out Vector2Int endCoordinate,
            out int endRegionIndex)
        {
            startCoordinate = default;
            endCoordinate = default;
            endRegionIndex = -1;

            int closestDistance = int.MaxValue;

            foreach (int connectedIndex in connectedRegionIndexes)
            {
                for (int regionIndex = 0; regionIndex < regionBoundaries.Count; regionIndex++)
                {
                    if (connectedRegionIndexes.Contains(regionIndex))
                    {
                        continue;
                    }

                    foreach (Vector2Int connectedBoundary in regionBoundaries[connectedIndex])
                    {
                        foreach (Vector2Int targetBoundary in regionBoundaries[regionIndex])
                        {
                            int distance = (connectedBoundary - targetBoundary).sqrMagnitude;

                            if (distance < closestDistance)
                            {
                                startCoordinate = connectedBoundary;
                                endCoordinate = targetBoundary;
                                endRegionIndex = regionIndex;
                                closestDistance = distance;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 두 동굴 중심 사이를 직선으로 이동하며 원형 영역을 굴착합니다.
        /// </summary>
        private void CarveTunnel(WorldMap worldMap, Vector2Int startCoordinate, Vector2Int endCoordinate, System.Random random)
        {
            Vector2 startPosition = startCoordinate;
            Vector2 endPosition = endCoordinate;
            Vector2 controlPosition = CreateControlPoint(worldMap, startPosition, endPosition, random);

            int stepCount = Mathf.CeilToInt(
                Vector2.Distance(startPosition, controlPosition) +
                Vector2.Distance(controlPosition, endPosition));

            for (int step = 0; step <= stepCount; step++)
            {
                float ratio = step / (float)stepCount;
                Vector2 position = CalculateBezierPoint(startPosition, controlPosition, endPosition, ratio);
                Vector2Int tunnelCoordinate = Vector2Int.RoundToInt(position);

                CarveCircle(worldMap, tunnelCoordinate);
            }
        }

        /// <summary>
        /// 시작점과 끝점 사이에서 곡선 방향을 결정하는 제어점을 만듭니다.
        /// </summary>
        private Vector2 CreateControlPoint(WorldMap worldMap, Vector2 startPosition, Vector2 endPosition, System.Random random)
        {
            Vector2 middlePosition = (startPosition + endPosition) * 0.5f;
            Vector2 direction = (endPosition - startPosition).normalized;
            Vector2 perpendicular = new Vector2(-direction.y, direction.x);

            float offset = Vector2.Distance(startPosition, endPosition) * curveOffsetRatio;
            Vector2 positiveControl = middlePosition + perpendicular * offset;
            Vector2 negativeControl = middlePosition - perpendicular * offset;

            bool isPositiveValid = IsCurveInsideWorld(worldMap, startPosition, positiveControl, endPosition);
            bool isNegativeValid = IsCurveInsideWorld(worldMap, startPosition, negativeControl, endPosition);

            if (isPositiveValid && isNegativeValid)
            {
                return random.Next(0, 2) == 0 ? positiveControl : negativeControl;
            }

            if (isPositiveValid)
            {
                return positiveControl;
            }

            if (isNegativeValid)
            {
                return negativeControl;
            }

            return middlePosition;
        }

        /// <summary>
        /// 2차 베지어 곡선 위의 좌표를 계산합니다.
        /// </summary>
        private static Vector2 CalculateBezierPoint(Vector2 startPosition, Vector2 controlPosition, Vector2 endPosition, float ratio)
        {
            float inverseRatio = 1f - ratio;

            return
                inverseRatio * inverseRatio * startPosition +
                2f * inverseRatio * ratio * controlPosition +
                ratio * ratio * endPosition;
        }

        /// <summary>
        /// 곡선에서 가장 많이 휘어지는 중간 지점이 월드 내부인지 확인합니다.
        /// </summary>
        private static bool IsCurveInsideWorld(WorldMap worldMap, Vector2 startPosition, Vector2 controlPosition, Vector2 endPosition)
        {
            Vector2 middlePosition = CalculateBezierPoint(startPosition, controlPosition, endPosition, 0.5f);
            Vector2Int middleCoordinate = Vector2Int.RoundToInt(middlePosition);

            if (middleCoordinate.x < 0 || middleCoordinate.y < 0)
            {
                return false;
            }

            return worldMap.TryGetTile(middleCoordinate, out _);
        }

        /// <summary>
        /// 중심 좌표 주변을 지정된 반경만큼 Empty로 변경합니다.
        /// </summary>
        private void CarveCircle(WorldMap worldMap, Vector2Int centerCoordinate)
        {
            for (int offsetY = -radius; offsetY <= radius; offsetY++)
            {
                for (int offsetX = -radius; offsetX <= radius; offsetX++)
                {
                    if (offsetX * offsetX + offsetY * offsetY > radius * radius)
                    {
                        continue;
                    }

                    Vector2Int targetCoordinate = centerCoordinate + new Vector2Int(offsetX, offsetY);

                    if (targetCoordinate.x < 0 || targetCoordinate.y < 0)
                    {
                        continue;
                    }

                    worldMap.TrySetTile(targetCoordinate, new WorldTile(WorldTileType.Empty));
                }
            }
        }

        /// <summary>
        /// Inspector 설정값을 유효한 범위로 보정합니다.
        /// </summary>
        public void Validate()
        {
            radius = Mathf.Max(1, radius);
        }
    }
}
