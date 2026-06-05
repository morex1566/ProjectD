using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    public partial class WorldManager : MonoBehaviourSingleton<WorldManager>
    {
        /// <summary>
        /// 지정 MapData의 타일을 생성하여 로드합니다.
        /// </summary>
        private void SpawnTilesInternal(MapData mapData)
        {
            if (mapData == null) return;

            Transform root = EnsureMapRoot();
            foreach (MapTileData tileData in mapData.Tiles)
            {
                if (tileData.TilePb == null) continue;

                TileController tile = Instantiate(tileData.TilePb, CellPosToWorldPos(tileData.CellPos), Quaternion.identity, root);

                tiles.Add(tileData.CellPos, tile);
                tile.CellPos = tileData.CellPos;
            }

            int topRowCellY = GetTopRowCellY(tiles.Keys);
            foreach (KeyValuePair<Vector3Int, TileController> pair in tiles)
            {
                if (pair.Value == null) continue;

                ApplyTileOrderInLayer(pair.Value, topRowCellY - pair.Key.y);
            }

            OnMapLoaded?.Invoke();
        }

        /// <summary>
        /// 월드 좌표에 대응하는 Ground CellPos를 반환합니다.
        /// </summary>
        private bool TryGetMapCellPosInternal(Vector3 worldPos, out Vector3Int cellPos)
        {
            cellPos = WorldToCellPos(worldPos);

            return tiles.ContainsKey(cellPos);
        }

        /// <summary>
        /// Ground CellPos가 유효하면 월드 중심 좌표를 반환합니다.
        /// </summary>
        private bool TryGetMapWorldPosInternal(Vector3Int cellPos, out Vector3 worldPos)
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
        private bool TryGetMapCenterWorldPosInternal(out Vector3 worldPos)
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

            // CellPos는 타일의 월드 중심이므로, 타일 월드 중심들의 경계 중간값을 사용합니다.
            worldPos = (minWorldPos + maxWorldPos) * 0.5f;
            return true;
        }

        /// <summary>
        /// 현재 로드된 맵의 열 개수를 반환합니다.
        /// </summary>
        private int GetMapColumnCountInternal()
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
        private int GetMapRowCountInternal()
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
        /// originCellPos 기준 현재 맵에서 이동가능한 CellPos를 가져옵니다.
        /// </summary>
        private List<Vector3Int> GetMovableCellPosListInternal(Vector3Int originCellPos, List<Vector3Int> directions, bool isRepeatable, bool isIncludeCreature)
        {
            List<Vector3Int> movableCellPosList = new();

            if (directions == null) return movableCellPosList;

            foreach (Vector3Int direction in directions)
            {
                if (direction == Vector3Int.zero) continue;

                Vector3Int candidateCellPos = originCellPos + direction;
                while (TryGetMapWorldPosInternal(candidateCellPos, out _))
                {
                    // 다른 크리처가 점유한 CellPos는 이동 가능 목록에서 제외하고, 반복 이동도 그 지점에서 멈춥니다.
                    if (!isIncludeCreature && HasCreatureInCellPos(candidateCellPos)) break;

                    if (movableCellPosList.Contains(candidateCellPos)) break;

                    movableCellPosList.Add(candidateCellPos);

                    if (!isRepeatable) break;

                    candidateCellPos += direction;
                }
            }

            return movableCellPosList;
        }

        /// <summary>
        /// 맵 타일을 담을 WorldManager 하위 루트를 준비합니다.
        /// </summary>
        private Transform EnsureMapRoot()
        {
            if (mapRoot != null) return mapRoot;

            GameObject rootObject = new GameObject("Map");
            rootObject.transform.SetParent(transform);
            rootObject.transform.localPosition = Vector3.zero;
            mapRoot = rootObject.transform;

            return mapRoot;
        }

        /// <summary>
        /// 현재 맵 타일 런타임 오브젝트와 조회 테이블을 정리합니다.
        /// </summary>
        private void UnloadMapTiles()
        {
            if (mapRoot != null)
            {
                Destroy(mapRoot.gameObject);
            }
            else
            {
                foreach (KeyValuePair<Vector3Int, TileController> pair in tiles)
                {
                    if (pair.Value == null) continue;

                    Destroy(pair.Value.gameObject);
                }
            }

            mapRoot = null;
            tiles.Clear();
        }

        /// <summary>
        /// 타일 밑 음영이 행 순서대로 가려지도록 최상단 행의 CellPos y를 찾습니다.
        /// </summary>
        private static int GetTopRowCellY(IEnumerable<Vector3Int> cellPosList)
        {
            bool hasCellPos = false;
            int topRowCellY = 0;
            foreach (Vector3Int cellPos in cellPosList)
            {
                if (!hasCellPos)
                {
                    topRowCellY = cellPos.y;
                    hasCellPos = true;
                    continue;
                }

                topRowCellY = Mathf.Max(topRowCellY, cellPos.y);
            }

            return hasCellPos ? topRowCellY : 0;
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
