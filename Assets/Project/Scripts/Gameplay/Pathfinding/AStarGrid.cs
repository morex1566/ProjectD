using System;
using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// WorldTileType 배열을 A* 탐색용 노드 그래프로 변환하고 경로를 계산합니다.
    /// </summary>
    public class AStarGrid
    {
        private const int StraightMoveCost = 10;
        private const int DiagonalMoveCost = 14;

        private AStarNode[,] nodes;

        public int Width { get; }

        public int Height { get; }

        public int NodeCount => Width * Height;

        /// <summary>
        /// WorldTileType 배열을 기반으로 이동 가능한 AStarNode 그리드를 생성합니다.
        /// </summary>
        public AStarGrid(WorldTileType[] mapTileTypes, int width, int height)
        {
            Width = width;
            Height = height;
            nodes = new AStarNode[width, height];

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    bool isWalkable = mapTileTypes[ToIndex(x, y)].HasFlag(WorldTileType.Air);
                    nodes[x, y] = new AStarNode(x, y, isWalkable);
                }
            }
        }

        /// <summary>
        /// IsInside인지?
        /// Walkable인지?
        /// </summary>
        public bool TryGetNode(int x, int y, out AStarNode node)
        {
            // 좌표가 그리드 좌표인지?
            node = null;
            if (!IsInside(x, y))
            {
                return false;
            }

            // 이동가능한 노드?
            node = GetNode(x, y);
            if (!node.IsWalkable)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 좌표에 해당하는 노드를 반환합니다.
        /// </summary>
        public AStarNode GetNode(int x, int y)
        {
            return nodes[x, y];
        }

        /// <summary>
        /// 2차원 배열을 1차원 배열로
        /// </summary>
        public int ToIndex(int x, int y)
        {
            return x + y * Width;
        }

        /// <summary>
        ///  1차원을 다시 2차원으로
        /// </summary>
        public int ToX(int index)
        {
            return index % Width;
        }

        /// <summary>
        ///  1차원을 다시 2차원으로
        /// </summary>
        public int ToY(int index)
        {
            return index / Width;
        }

        /// <summary>
        /// 좌표가 그리드 안 좌표임?
        /// </summary>
        public bool IsInside(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        /// <summary>
        /// 길찾기 시작
        /// </summary>
        public List<AStarNode> FindPath(AStarNode startNode, AStarNode targetNode)
        {
            // 이전 탐색에서 남은 G/H/Parent 값을 초기화한다.
            ResetNodes();

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
                foreach (AStarNode neighbor in GetNeighbors(currentNode))
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
        public List<AStarNode> GetNeighbors(AStarNode node)
        {
            List<AStarNode> neighbors = new List<AStarNode>();

            AddNeighbor(neighbors, node, node.X, node.Y + 1);
            AddNeighbor(neighbors, node, node.X + 1, node.Y + 1);
            AddNeighbor(neighbors, node, node.X + 1, node.Y);
            AddNeighbor(neighbors, node, node.X + 1, node.Y - 1);
            AddNeighbor(neighbors, node, node.X, node.Y - 1);
            AddNeighbor(neighbors, node, node.X - 1, node.Y - 1);
            AddNeighbor(neighbors, node, node.X - 1, node.Y);
            AddNeighbor(neighbors, node, node.X - 1, node.Y + 1);

            return neighbors;
        }

        /// <summary>
        /// A*는 탐색 중 노드에 상태를 저장함
        /// 다음 탐색 전에 이전 상태를 지워야 함
        /// </summary>
        public void ResetNodes()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    AStarNode node = nodes[x, y];

                    node.GCost = int.MaxValue;
                    node.HCost = 0;
                    node.Parent = null;
                }
            }
        }




        /// <summary>
        /// 후보 좌표가 이동 가능한 이웃인지 검사한 뒤 목록에 추가합니다.
        /// </summary>
        private void AddNeighbor(List<AStarNode> neighbors, AStarNode from, int x, int y)
        {
            if (!TryGetNode(x, y, out AStarNode node))
            {
                return;
            }

            if (!EvaluateNeighbor(from, node))
            {
                return;
            }

            neighbors.Add(node);
        }

        /// <summary>
        /// 대각선 이동 시 수직 방향에 발판이 있는지 검사해 허용 여부를 결정합니다.
        /// </summary>
        private bool EvaluateNeighbor(AStarNode from, AStarNode to)
        {
            int moveY = to.Y - from.Y;

            if (moveY == 0 || to.X == from.X)
            {
                return true;
            }

            // 대각선 이동 시 진행 방향의 바로 위/아래 칸이 막혀 있으면 이동하지 않습니다.
            return TryGetNode(from.X, from.Y + moveY, out _);
        }

        /// <summary>
        /// 두 노드의 이동이 대각선인지 확인합니다.
        /// </summary>
        private bool IsDiagonalMove(AStarNode from, AStarNode to)
        {
            return from.X != to.X && from.Y != to.Y;
        }

        /// <summary>
        /// 대각 이동을 허용하는 Octile 거리 비용을 계산합니다.
        /// </summary>
        private int CalculateOctileDistance(int distanceX, int distanceY)
        {
            int diagonalCount = Math.Min(distanceX, distanceY);
            int straightCount = Math.Abs(distanceX - distanceY);

            return diagonalCount * DiagonalMoveCost + straightCount * StraightMoveCost;
        }

        /// <summary>
        /// 현재 노드에서 목표 노드까지의 휴리스틱 비용을 계산합니다.
        /// </summary>
        private int CalculateHeuristic(AStarNode current, AStarNode target)
        {
            int distanceX = Math.Abs(current.X - target.X);
            int distanceY = Math.Abs(current.Y - target.Y);

            return CalculateOctileDistance(distanceX, distanceY);
        }

        /// <summary>
        /// 실제 이동 방향에 따른 한 칸 이동 비용을 반환합니다.
        /// </summary>
        private int CalculateMoveCost(AStarNode from, AStarNode to)
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
        private AStarNode GetLowestCostNode(List<AStarNode> openList)
        {
            AStarNode bestNode = openList[0];

            for (int i = 1; i < openList.Count; i++)
            {
                AStarNode candidate = openList[i];

                if (candidate.FCost < bestNode.FCost ||
                    candidate.FCost == bestNode.FCost && candidate.HCost < bestNode.HCost)
                {
                    bestNode = candidate;
                }
            }

            return bestNode;
        }

        /// <summary>
        /// 뒤집어진 경로를 백트래이싱
        /// </summary>
        private List<AStarNode> BuildPath(AStarNode targetNode)
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
