using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 현재 WorldMap의 지형 타일을 클릭 또는 드래그로 선택합니다.
    /// </summary>
    public sealed class WorldTileSelector : Selector<Vector2Int>
    {
        [SerializeField] private Sprite selectedTileSprite = null;

        private readonly HashSet<Vector2Int> selectedCoordinates = new();

        private readonly Dictionary<Vector2Int, GameObject> selectionUIMap = new();


        /// <summary>
        /// 드래그 화면 영역에 들어온 지형 타일의 선택 표시를 갱신합니다.
        /// </summary>
        protected override void SelectTargets(Camera cam, Vector2 startScreenPosition, Vector2 endScreenPosition)
        {
            SetSelection(FindSelectableTiles(cam, startScreenPosition, endScreenPosition));
        }

        /// <summary>
        /// 포인터 아래의 지형 타일 하나의 선택 표시를 갱신합니다.
        /// </summary>
        protected override void SelectTarget(Camera cam, Vector2 pointerWorldPosition)
        {
            HashSet<Vector2Int> currentSelectedCoordinates = new();

            if (TryGetSelectableTile(pointerWorldPosition, out Vector2Int tileCoordinate) == true)
            {
                currentSelectedCoordinates.Add(tileCoordinate);
            }

            SetSelection(currentSelectedCoordinates);
        }

        /// <summary>
        /// 현재 선택 표시가 남아 있는 지형 타일을 확정 Target으로 등록합니다.
        /// </summary>
        protected override void CompleteSelection()
        {
            targets.Clear();

            foreach (Vector2Int coordinate in selectedCoordinates)
            {
                AddTarget(coordinate);
            }
        }

        /// <summary>
        /// 현재 확정 선택 목록과 타일 선택 표시를 제거합니다.
        /// </summary>
        protected override void ClearTarget()
        {
            foreach (GameObject selectionUI in selectionUIMap.Values)
            {
                if (selectionUI != null)
                {
                    Destroy(selectionUI);
                }
            }

            selectionUIMap.Clear();
            selectedCoordinates.Clear();
            targets.Clear();
        }

        /// <summary>
        /// 확정 Target 목록에 타일 좌표를 중복 없이 추가합니다.
        /// </summary>
        protected override void AddTarget(Vector2Int selectedTarget)
        {
            if (targets.Contains(selectedTarget) == false)
            {
                targets.Add(selectedTarget);
            }
        }

        /// <summary>
        /// 현재 드래그 또는 클릭 범위와 타일 선택 표시 상태를 동기화합니다.
        /// </summary>
        private void SetSelection(HashSet<Vector2Int> currentSelectedCoordinates)
        {
            List<Vector2Int> removedCoordinates = new();

            foreach (Vector2Int coordinate in selectedCoordinates)
            {
                if (currentSelectedCoordinates.Contains(coordinate) == false)
                {
                    removedCoordinates.Add(coordinate);
                }
            }

            foreach (Vector2Int coordinate in removedCoordinates)
            {
                selectedCoordinates.Remove(coordinate);
                RemoveSelectionUI(coordinate);
            }

            foreach (Vector2Int coordinate in currentSelectedCoordinates)
            {
                if (selectedCoordinates.Add(coordinate) == false)
                {
                    continue;
                }

                if (TrySetSelectionUI(coordinate) == false)
                {
                    selectedCoordinates.Remove(coordinate);
                }
            }
        }

        /// <summary>
        /// 지정한 타일 중심에 선택 표시를 생성합니다.
        /// </summary>
        private bool TrySetSelectionUI(Vector2Int coordinate)
        {
            WorldGenerationSettingsData settings = WorldManager.Settings?.WorldGenerationSettingsData;
            if (settings == null || selectedTileSprite == null)
            {
                return false;
            }

            GameObject selectionUI = new GameObject("selection_tile");
            selectionUI.transform.SetParent(transform, false);
            selectionUI.transform.position = new Vector3(
                (coordinate.x + 0.5f) * settings.TileWorldSize,
                (coordinate.y + 0.5f) * settings.TileWorldSize,
                0f);

            SpriteRenderer selectionRenderer = selectionUI.AddComponent<SpriteRenderer>();
            selectionRenderer.sprite = selectedTileSprite;
            selectionRenderer.sortingOrder = 1;

            selectionUIMap[coordinate] = selectionUI;
            return true;
        }

        /// <summary>
        /// 지정한 타일의 선택 표시를 제거합니다.
        /// </summary>
        private void RemoveSelectionUI(Vector2Int coordinate)
        {
            if (selectionUIMap.TryGetValue(coordinate, out GameObject selectionUI) == false)
            {
                return;
            }

            selectionUIMap.Remove(coordinate);

            if (selectionUI != null)
            {
                Destroy(selectionUI);
            }
        }

        /// <summary>
        /// 드래그 화면 영역에 중심점이 들어온 비어 있지 않은 타일을 반환합니다.
        /// </summary>
        private static HashSet<Vector2Int> FindSelectableTiles(Camera cam, Vector2 startScreenPosition, Vector2 endScreenPosition)
        {
            HashSet<Vector2Int> coordinates = new();

            if (WorldManager.TryGetWorldMap(out WorldMap worldMap) == false)
            {
                return coordinates;
            }

            WorldGenerationSettingsData settings = WorldManager.Settings?.WorldGenerationSettingsData;
            if (settings == null ||
                MouseEx.TryGetWorldPosition(cam, startScreenPosition, out Vector3 startWorldPosition) == false ||
                MouseEx.TryGetWorldPosition(cam, endScreenPosition, out Vector3 endWorldPosition) == false)
            {
                return coordinates;
            }

            Rect selectionRect = ScreenEx.CreateScreenRect(startScreenPosition, endScreenPosition);
            Vector2Int startCoordinate = WorldToTileCoordinate(startWorldPosition, settings.TileWorldSize);
            Vector2Int endCoordinate = WorldToTileCoordinate(endWorldPosition, settings.TileWorldSize);
            Vector2Int minimum = Vector2Int.Min(startCoordinate, endCoordinate);
            Vector2Int maximum = Vector2Int.Max(startCoordinate, endCoordinate);
            Vector2Int worldMaximum = settings.ChunkSize * settings.TilesPerChunk - Vector2Int.one;

            minimum = Vector2Int.Max(Vector2Int.zero, minimum);
            maximum = Vector2Int.Min(worldMaximum, maximum);

            if (minimum.x > maximum.x || minimum.y > maximum.y)
            {
                return coordinates;
            }

            for (int y = minimum.y; y <= maximum.y; y++)
            {
                for (int x = minimum.x; x <= maximum.x; x++)
                {
                    Vector2Int coordinate = new Vector2Int(x, y);

                    if (TryGetSelectableTile(worldMap, coordinate, out _) == false)
                    {
                        continue;
                    }

                    Vector3 tileCenterWorldPosition = new Vector3(
                        (coordinate.x + 0.5f) * settings.TileWorldSize,
                        (coordinate.y + 0.5f) * settings.TileWorldSize,
                        0f);
                    Vector2 tileCenterScreenPosition = cam.WorldToScreenPoint(tileCenterWorldPosition);

                    if (selectionRect.Contains(tileCenterScreenPosition) == true)
                    {
                        coordinates.Add(coordinate);
                    }
                }
            }

            return coordinates;
        }

        /// <summary>
        /// 월드 위치 아래의 비어 있지 않은 타일 좌표를 반환합니다.
        /// </summary>
        private static bool TryGetSelectableTile(Vector2 pointerWorldPosition, out Vector2Int tileCoordinate)
        {
            tileCoordinate = default;

            if (WorldManager.TryGetWorldMap(out WorldMap worldMap) == false)
            {
                return false;
            }

            WorldGenerationSettingsData settings = WorldManager.Settings?.WorldGenerationSettingsData;
            if (settings == null)
            {
                return false;
            }

            tileCoordinate = WorldToTileCoordinate(pointerWorldPosition, settings.TileWorldSize);
            return TryGetSelectableTile(worldMap, tileCoordinate, out _);
        }

        /// <summary>
        /// 지정한 좌표에 비어 있지 않은 월드 타일이 있는지 확인합니다.
        /// </summary>
        private static bool TryGetSelectableTile(WorldMap worldMap, Vector2Int coordinate, out WorldTile tile)
        {
            tile = default;

            if (coordinate.x < 0 || coordinate.y < 0)
            {
                return false;
            }

            return worldMap.TryGetTile(coordinate, out tile) == true && tile.IsEmpty == false;
        }

        /// <summary>
        /// 월드 위치를 현재 월드의 전역 타일 좌표로 변환합니다.
        /// </summary>
        private static Vector2Int WorldToTileCoordinate(Vector2 worldPosition, float tileWorldSize)
        {
            return new Vector2Int(
                Mathf.FloorToInt(worldPosition.x / tileWorldSize),
                Mathf.FloorToInt(worldPosition.y / tileWorldSize));
        }
    }
}
