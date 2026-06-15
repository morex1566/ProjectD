using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    public class AStarPathfinder : MonoBehaviour
    {
        private static AStarGrid astarGrid;

        private void Start()
        {
            var mapGenerater = GetComponent<MapGenerator>();
            astarGrid = new AStarGrid(mapGenerater.MapWidth, mapGenerater.MapHeight);
        }

        /// <summary>
        /// 길찾기 시작
        /// </summary>
        public static List<AStarNode> FindPath(Vector3Int startPos, Vector3Int targetPos)
        {
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