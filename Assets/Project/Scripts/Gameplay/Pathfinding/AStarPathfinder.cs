using System;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 현재 MapController의 타일 배열로 AStarGrid를 만들고 전역 경로 탐색 진입점을 제공합니다.
    /// </summary>
    public class AStarPathfinder : MonoBehaviour
    {
        [SerializeField] private int width = 10;

        [SerializeField] private int height = 10;

        private const int StraightMoveCost = 10;
        private const int DiagonalMoveCost = 14;

        private static AStarGrid astarGrid;

        public static AStarGrid AStarGrid => astarGrid;

        /// <summary>
        /// 같은 GameObject의 MapController에서 길찾기 그리드를 초기화합니다.
        /// </summary>
        private void Start()
        {
            // Map의 타일 타입에 맞춰서 그리드 생성
            var gridController = GetComponent<WorldGridController>();
            Generate();
        }

        [ContextMenu(nameof(Generate))]
        public void Generate()
        {
            astarGrid = new AStarGrid(width, height);
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

            return FindPath(astarGrid, startNode, targetNode);
        }

        /// <summary>
        /// 길찾기 시작
        /// </summary>
        private static List<AStarNode> FindPath(AStarGrid grid, AStarNode startNode, AStarNode targetNode)
        {
            // 이전 탐색에서 남은 G/H/Parent 값을 초기화한다.
            ResetNodes(grid);

            // Open: 앞으로 검사할 후보 노드들
            // Closed: 이미 검사가 끝난 노드들
            List<AStarNode> openList = new List<AStarNode>();
            HashSet<AStarNode> closedSet = new HashSet<AStarNode>();

            // 시작 노드의 비용을 설정하고 탐색 후보에 넣는다.
            startNode.GCost = 0;
            startNode.HCost = CalculateHeuristic(startNode, targetNode);
            startNode.Parent = null;
            openList.Add(startNode);

            // 더 이상 검사할 후보가 없을 때까지 반복한다.
            while (openList.Count > 0)
            {
                // 후보 중 FCost가 가장 낮은 노드를 현재 노드로 선택한다.
                AStarNode currentNode = GetLowestCostNode(openList);

                // 목표 노드에 도착했다면 Parent를 따라가며 최종 경로를 만든다.
                if (currentNode == targetNode)
                {
                    return BuildPath(targetNode);
                }

                // 현재 노드는 검사가 끝났으므로 Open에서 제거하고 Closed에 넣는다.
                openList.Remove(currentNode);
                closedSet.Add(currentNode);

                // 현재 노드에서 이동 가능한 주변 노드를 확인한다.
                foreach (AStarNode neighbor in GetNeighbors(grid, currentNode))
                {
                    // 이미 검사 완료된 노드는 다시 검사하지 않는다.
                    if (closedSet.Contains(neighbor))
                    {
                        continue;
                    }

                    // 현재 노드를 거쳐 이웃 노드로 가는 새 비용을 계산한다.
                    int newGCost = currentNode.GCost + CalculateMoveCost(currentNode, neighbor);

                    // 기존 경로가 더 싸거나 같으면 갱신하지 않는다.
                    if (newGCost >= neighbor.GCost)
                    {
                        continue;
                    }

                    // 더 좋은 경로를 찾았으므로 비용과 이전 노드를 갱신한다.
                    neighbor.GCost = newGCost;
                    neighbor.HCost = CalculateHeuristic(neighbor, targetNode);
                    neighbor.Parent = currentNode;

                    // 아직 후보 목록에 없다면 추가한다.
                    if (!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
                    }
                }
            }

            // Open이 비었다는 것은 목표까지 갈 수 있는 경로가 없다는 뜻이다.
            return null;
        }

        /// <summary>
        /// 길찾기 로직중에 현재 노드의 주변 노드를 탐색
        /// </summary>
        private static List<AStarNode> GetNeighbors(AStarGrid grid, AStarNode node)
        {
            List<AStarNode> neighbors = new List<AStarNode>();

            AddNeighbor(grid, neighbors, node, node.X, node.Y + 1);
            AddNeighbor(grid, neighbors, node, node.X + 1, node.Y + 1);
            AddNeighbor(grid, neighbors, node, node.X + 1, node.Y);
            AddNeighbor(grid, neighbors, node, node.X + 1, node.Y - 1);
            AddNeighbor(grid, neighbors, node, node.X, node.Y - 1);
            AddNeighbor(grid, neighbors, node, node.X - 1, node.Y - 1);
            AddNeighbor(grid, neighbors, node, node.X - 1, node.Y);
            AddNeighbor(grid, neighbors, node, node.X - 1, node.Y + 1);

            return neighbors;
        }

        /// <summary>
        /// A*는 탐색 중 노드에 상태를 저장함
        /// 다음 탐색 전에 이전 상태를 지워야 함
        /// </summary>
        private static void ResetNodes(AStarGrid grid)
        {
            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    AStarNode node = grid.GetNode(x, y);

                    node.GCost = int.MaxValue;
                    node.HCost = 0;
                    node.Parent = null;
                }
            }
        }

        /// <summary>
        /// 후보 좌표가 이동 가능한 이웃인지 검사한 뒤 목록에 추가합니다.
        /// </summary>
        private static void AddNeighbor(AStarGrid grid, List<AStarNode> neighbors, AStarNode from, int x, int y)
        {
            if (!grid.TryGetNode(x, y, out AStarNode node))
            {
                return;
            }

            if (!EvaluateNeighbor(grid, from, node))
            {
                return;
            }

            neighbors.Add(node);
        }

        /// <summary>
        /// 대각선 이동 시 수직 방향에 발판이 있는지 검사해 허용 여부를 결정합니다.
        /// </summary>
        private static bool EvaluateNeighbor(AStarGrid grid, AStarNode from, AStarNode to)
        {
            int moveY = to.Y - from.Y;

            if (moveY == 0 || to.X == from.X)
            {
                return true;
            }

            // 대각선 이동 시 진행 방향의 바로 위/아래 칸이 막혀 있으면 이동하지 않습니다.
            return grid.TryGetNode(from.X, from.Y + moveY, out _);
        }

        /// <summary>
        /// 두 노드의 이동이 대각선인지 확인합니다.
        /// </summary>
        private static bool IsDiagonalMove(AStarNode from, AStarNode to)
        {
            return from.X != to.X && from.Y != to.Y;
        }

        /// <summary>
        /// 대각 이동을 허용하는 Octile 거리 비용을 계산합니다.
        /// </summary>
        private static int CalculateOctileDistance(int distanceX, int distanceY)
        {
            int diagonalCount = Math.Min(distanceX, distanceY);
            int straightCount = Math.Abs(distanceX - distanceY);

            return diagonalCount * DiagonalMoveCost + straightCount * StraightMoveCost;
        }

        /// <summary>
        /// 현재 노드에서 목표 노드까지의 휴리스틱 비용을 계산합니다.
        /// </summary>
        private static int CalculateHeuristic(AStarNode current, AStarNode target)
        {
            int distanceX = Math.Abs(current.X - target.X);
            int distanceY = Math.Abs(current.Y - target.Y);

            return CalculateOctileDistance(distanceX, distanceY);
        }

        /// <summary>
        /// 실제 이동 방향에 따른 한 칸 이동 비용을 반환합니다.
        /// </summary>
        private static int CalculateMoveCost(AStarNode from, AStarNode to)
        {
            if (IsDiagonalMove(from, to))
            {
                return DiagonalMoveCost;
            }

            return StraightMoveCost;
        }

        /// <summary>
        /// openList 안에서 FCost가 가장 낮은 노드를 찾습니다.
        /// 동점이면 HCost가 낮은 쪽을 고릅니다.
        /// </summary>
        private static AStarNode GetLowestCostNode(List<AStarNode> openList)
        {
            AStarNode bestNode = openList[0];

            for (int i = 1; i < openList.Count; i++)
            {
                AStarNode candidate = openList[i];

                int candidateFCost = CalculateFCost(candidate);
                int bestFCost = CalculateFCost(bestNode);

                if (candidateFCost < bestFCost ||
                    candidateFCost == bestFCost && candidate.HCost < bestNode.HCost)
                {
                    bestNode = candidate;
                }
            }

            return bestNode;
        }

        /// <summary>
        /// 노드에 저장된 G/H 비용으로 우선순위 비용을 계산합니다.
        /// </summary>
        private static int CalculateFCost(AStarNode node)
        {
            return node.GCost + node.HCost;
        }

        /// <summary>
        /// 뒤집어진 경로를 백트래이싱
        /// </summary>
        private static List<AStarNode> BuildPath(AStarNode targetNode)
        {
            List<AStarNode> path = new List<AStarNode>();

            AStarNode currentNode = targetNode;

            while (currentNode != null)
            {
                path.Add(currentNode);
                currentNode = currentNode.Parent;
            }

            path.Reverse();

            return path;
        }
    }
}
