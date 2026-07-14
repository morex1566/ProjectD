using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 월드 타일의 연결된 공간을 탐색합니다.
    /// </summary>
    public static class WorldPathfinder
    {
        private static readonly Vector2Int[] neighborDirections =
        {
            Vector2Int.left,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.up,
        };

        /// <summary>
        /// 맵 외곽과 연결되지 않은 Empty 영역을 동굴로 반환합니다.
        /// </summary>
        public static List<List<Vector2Int>> FindCaveRegions(WorldMap worldMap)
        {
            List<List<Vector2Int>> caveRegions = new List<List<Vector2Int>>();
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

            foreach (WorldChunk chunk in worldMap.Chunks.Values)
            {
                FindChunkCaveRegions(worldMap, chunk, visited, caveRegions);
            }

            return caveRegions;
        }

        /// <summary>
        /// 청크에 포함된 아직 방문하지 않은 Empty 영역을 탐색합니다.
        /// </summary>
        private static void FindChunkCaveRegions(WorldMap worldMap, WorldChunk chunk, HashSet<Vector2Int> visited, List<List<Vector2Int>> caveRegions)
        {
            for (int localY = 0; localY < WorldChunk.Size; localY++)
            {
                for (int localX = 0; localX < WorldChunk.Size; localX++)
                {
                    WorldTile tile = chunk.GetTile(localX, localY);

                    // 빈 공간이 아닌 타일은 동굴 탐색 대상이 아니므로 건너뜁니다.
                    if (tile.IsEmpty == false)
                    {
                        continue;
                    }

                    // 이미 다른 Flood Fill에서 확인한 좌표라면 다시 탐색하지 않습니다.
                    Vector2Int worldCoordinate = chunk.Coordinate * WorldChunk.Size + new Vector2Int(localX, localY);
                    if (visited.Contains(worldCoordinate))
                    {
                        continue;
                    }

                    // 월드 외곽과 연결된 빈 공간은 동굴이 아니라 외부 공간으로 판단합니다.
                    List<Vector2Int> region = FloodFill(worldMap, worldCoordinate, visited, out bool touchesMapBoundary);
                    if (touchesMapBoundary == true)
                    {
                        continue;
                    }

                    caveRegions.Add(region);
                }
            }
        }

        /// <summary>
        /// 시작 좌표와 상하좌우로 연결된 모든 Empty 타일을 찾습니다.
        /// </summary>
        private static List<Vector2Int> FloodFill(WorldMap worldMap, Vector2Int startCoordinate, HashSet<Vector2Int> visited, out bool touchesMapBoundary)
        {
            List<Vector2Int> region = new List<Vector2Int>();
            Queue<Vector2Int> openCoordinates = new Queue<Vector2Int>();
            {
                region.Add(startCoordinate);
                openCoordinates.Enqueue(startCoordinate);
                visited.Add(startCoordinate);
            }

            touchesMapBoundary = false;

            while (openCoordinates.Count > 0)
            {
                Vector2Int currentCoordinate = openCoordinates.Dequeue();

                foreach (Vector2Int direction in neighborDirections)
                {
                    Vector2Int neighborCoordinate = currentCoordinate + direction;

                    if (neighborCoordinate.x < 0 || neighborCoordinate.y < 0)
                    {
                        touchesMapBoundary = true;
                        continue;
                    }

                    if (worldMap.TryGetTile(neighborCoordinate, out WorldTile neighborTile) == false)
                    {
                        touchesMapBoundary = true;
                        continue;
                    }

                    if (neighborTile.IsEmpty == false || visited.Add(neighborCoordinate) == false)
                    {
                        continue;
                    }

                    region.Add(neighborCoordinate);
                    openCoordinates.Enqueue(neighborCoordinate);
                }
            }

            return region;
        }
    }
}
