using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 현재 MapController의 타일 배열로 AStarGrid를 만들고 전역 경로 탐색 진입점을 제공합니다.
    /// </summary>
    public class AStarPathfinder : MonoBehaviour
    {
        private static AStarGrid astarGrid;

        /// <summary>
        /// 같은 GameObject의 MapController에서 길찾기 그리드를 초기화합니다.
        /// </summary>
        private void Start()
        {
            // Map의 타일 타입에 맞춰서 그리드 생성
            var mapController = GetComponent<MapController>();
            astarGrid = new AStarGrid(mapController.TileTypes, mapController.MapWidth, mapController.MapHeight);
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
    }
}
