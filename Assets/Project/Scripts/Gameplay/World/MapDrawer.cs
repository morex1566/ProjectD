using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TRPG.Runtime
{
    /// <summary>
    /// MapBrush의 패턴을 사용해 Tilemap에 타일을 배치합니다.
    /// </summary>
    public class MapDrawer : MonoBehaviour
    {
        [SerializeField] private Tilemap targetTilemap;

        [SerializeField] private MapBrush brush;

        /// <summary>
        /// 지정한 셀에 패턴에서 선택된 타일 하나를 배치합니다.
        /// </summary>
        public bool Draw(Vector3Int cellPos)
        {
            // 그릴 곳 있음?
            if (targetTilemap == null || brush == null) return false;

            // 브러시에 타일 등록 되어있음?
            if (!brush.TryGetRandomTile(out TileBase tile)) return false;

            targetTilemap.SetTile(cellPos, tile);

            return true;
        }

        /// <summary>
        /// 여러 셀에 같은 패턴을 각각 독립적으로 추첨해 배치합니다.
        /// </summary>
        public int Draw(IReadOnlyList<Vector3Int> cellPoss)
        {
            if (cellPoss == null) return 0;

            int drawnCount = 0;

            for (int i = 0; i < cellPoss.Count; i++)
            {
                if (Draw(cellPoss[i]))
                {
                    drawnCount++;
                }
            }

            return drawnCount;
        }
    }
}
