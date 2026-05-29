using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 맵 데이터 기반 타일 인스턴스와 맵 좌표 변환을 관리합니다.
    /// </summary>
    public class MapController : MonoBehaviour
    {
        [Header(nameof(MapController) + ".Runtime")]

        [SerializeField, ReadOnly] private MapData currMapData = null;

        [SerializeField, ReadOnly] private Dictionary<Vector3Int, TileController> tiles = new();


        public MapData CurrMapData => currMapData;


        /// <summary>
        /// 월드 좌표에 대응하는 유효한 맵 CellPos를 찾습니다.
        /// </summary>
        public bool TryGetMapCellPos(Vector3 worldPos, out Vector3Int cellPos)
        {
            cellPos = WorldToCellPos(worldPos);

            return tiles.ContainsKey(cellPos);
        }

        /// <summary>
        /// 맵 CellPos에 대응하는 월드 중심 좌표를 찾습니다.
        /// </summary>
        public bool TryGetMapWorldPos(Vector3Int cellPos, out Vector3 worldPos)
        {
            if (!tiles.ContainsKey(cellPos))
            {
                worldPos = default;
                return false;
            }

            worldPos = CellPosToWorldPos(cellPos);
            return true;
        }

        /// <summary>
        /// 현재 로드된 맵의 월드 기준 정중앙 좌표를 찾습니다.
        /// </summary>
        public bool TryGetMapCenterWorldPos(out Vector3 worldPos)
        {
            bool hasTile = false;
            Vector3 minWorldPos = default;
            Vector3 maxWorldPos = default;

            foreach (Vector3Int cellPos in tiles.Keys)
            {
                Vector3 tileWorldPos = CellPosToWorldPos(cellPos);
                if (!hasTile)
                {
                    minWorldPos = tileWorldPos;
                    maxWorldPos = tileWorldPos;
                    hasTile = true;
                    continue;
                }

                minWorldPos = Vector3.Min(minWorldPos, tileWorldPos);
                maxWorldPos = Vector3.Max(maxWorldPos, tileWorldPos);
            }

            if (!hasTile)
            {
                worldPos = default;
                return false;
            }

            // CellPos는 타일의 월드 중심이므로, CellPos가 아니라 타일 월드 중심들의 경계 중간값을 사용합니다.
            worldPos = (minWorldPos + maxWorldPos) * 0.5f;
            return true;
        }

        /// <summary>
        /// 현재 로드된 맵의 열 개수를 반환합니다.
        /// </summary>
        public int GetMapColumnCount()
        {
            bool hasTile = false;
            int minCellX = 0;
            int maxCellX = 0;

            foreach (Vector3Int cellPos in tiles.Keys)
            {
                if (!hasTile)
                {
                    minCellX = cellPos.x;
                    maxCellX = cellPos.x;
                    hasTile = true;
                    continue;
                }

                minCellX = Mathf.Min(minCellX, cellPos.x);
                maxCellX = Mathf.Max(maxCellX, cellPos.x);
            }

            if (!hasTile) return 0;

            // 열 수는 맵의 x축 경계 너비입니다. 중간에 비어 있는 CellPos가 있어도 전체 맵 폭으로 계산합니다.
            return maxCellX - minCellX + 1;
        }

        /// <summary>
        /// 현재 로드된 맵의 행 개수를 반환합니다.
        /// </summary>
        public int GetMapRowCount()
        {
            bool hasTile = false;
            int minCellY = 0;
            int maxCellY = 0;

            foreach (Vector3Int cellPos in tiles.Keys)
            {
                if (!hasTile)
                {
                    minCellY = cellPos.y;
                    maxCellY = cellPos.y;
                    hasTile = true;
                    continue;
                }

                minCellY = Mathf.Min(minCellY, cellPos.y);
                maxCellY = Mathf.Max(maxCellY, cellPos.y);
            }

            if (!hasTile) return 0;

            // 행 수는 맵의 y축 경계 높이입니다. 중간에 비어 있는 CellPos가 있어도 전체 맵 높이로 계산합니다.
            return maxCellY - minCellY + 1;
        }

        /// <summary>
        /// 월드 좌표를 논리 CellPos로 변환합니다.
        /// </summary>
        public static Vector3Int WorldToCellPos(Vector3 worldPos)
        {
            // 타일 크기는 1입니다. WorldPosition (0, 0, 0)은 CellPos (0, 0)에 매핑되고 z는 논리 좌표에서 사용하지 않습니다.
            return new Vector3Int(
                Mathf.FloorToInt(worldPos.x + 0.5f),
                Mathf.FloorToInt(worldPos.y + 0.5f),
                0);
        }

        /// <summary>
        /// 논리 CellPos를 월드 중심 좌표로 변환합니다.
        /// </summary>
        public static Vector3 CellPosToWorldPos(Vector3Int cellPos)
        {
            return new Vector3(cellPos.x, cellPos.y, cellPos.z);
        }

        /// <summary>
        /// 맵 데이터를 타일 오브젝트로 인스턴스화하고 조회 테이블을 갱신합니다.
        /// </summary>
        public void LoadMapData(MapData mapData)
        {
            if (mapData == null) return;

            UnloadMapData();
            currMapData = mapData;

            int topRowCellY = GetTopRowCellY(mapData.Tiles);
            foreach (MapTileData tileData in mapData.Tiles)
            {
                if (tileData.TilePb == null) continue;

                TileController tile = Instantiate(tileData.TilePb, CellPosToWorldPos(tileData.CellPos), Quaternion.identity, transform);

                ApplyTileOrderInLayer(tile, topRowCellY - tileData.CellPos.y);
                tiles.Add(tileData.CellPos, tile);
            }
        }

        /// <summary>
        /// 현재 맵 타일 런타임 오브젝트와 조회 테이블을 정리합니다.
        /// </summary>
        public void UnloadMapData()
        {
            foreach (KeyValuePair<Vector3Int, TileController> pair in tiles)
            {
                if (pair.Value == null) continue;

                Destroy(pair.Value.gameObject);
            }

            currMapData = null;
            tiles.Clear();
        }

        /// <summary>
        /// 타일 밑 음영이 행 순서대로 가려지도록 최상단 행의 CellPos y를 찾습니다.
        /// </summary>
        private static int GetTopRowCellY(IReadOnlyList<MapTileData> tileDataList)
        {
            if (tileDataList.Count == 0) return 0;

            int topRowCellY = tileDataList[0].CellPos.y;
            foreach (MapTileData tileData in tileDataList)
            {
                topRowCellY = Mathf.Max(topRowCellY, tileData.CellPos.y);
            }

            return topRowCellY;
        }

        /// <summary>
        /// CellPos y가 큰 최상단 행부터 SpriteRenderer Order in Layer를 0, 1, 2...로 배정합니다.
        /// </summary>
        private static void ApplyTileOrderInLayer(TileController tile, int baseOrderInLayer)
        {
            SpriteRenderer[] renderers = tile.GetComponentsInChildren<SpriteRenderer>(true);
            if (renderers.Length == 0) return;

            int minOrderInLayer = renderers[0].sortingOrder;
            foreach (SpriteRenderer renderer in renderers)
            {
                minOrderInLayer = Mathf.Min(minOrderInLayer, renderer.sortingOrder);
            }

            foreach (SpriteRenderer renderer in renderers)
            {
                int relativeOrderInLayer = renderer.sortingOrder - minOrderInLayer;
                renderer.sortingOrder = baseOrderInLayer + relativeOrderInLayer;
            }
        }
    }
}
