using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TRPG.Runtime
{
    /// <summary>
    /// 타일 클릭 선택과 드래그 선택 로직을 처리합니다.
    /// </summary>
    public class WorldTileSelector : Selector<Vector3Int>
    {
        [SerializeField] private TileBase selectedWithBorderTileBase;

        private readonly List<Vector3Int> previewSelecteds = new();

        private bool isPreviewing;

        /// <summary>
        /// 공사 선택 모드 진입 시 이전 타일 선택을 초기화합니다.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            Clear();
        }

        /// <summary>
        /// 공사 선택 모드 종료 시 선택 타일 표시를 제거합니다.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            Clear();
        }

        /// <summary>
        /// 드래그 중 화면 영역과 겹치는 실제 타일 셀들을 임시 표시합니다.
        /// </summary>
        protected override void SelectPreviews(Camera cam, Vector2 startScreenPos, Vector2 endScreenPos)
        {
            SetPreviewSelection(FindSelectableCells(cam, startScreenPos, endScreenPos));
        }

        /// <summary>
        /// 클릭 홀드 중 포인터 아래의 타일 셀을 임시 표시합니다.
        /// </summary>
        protected override void SelectPreview(Camera cam, Vector2 mouseWorldPosition)
        {
            HashSet<Vector3Int> currentPreviewSelecteds = new();
            if (TryGetSelectableCell(mouseWorldPosition, out Vector3Int cellPos) == true)
            {
                currentPreviewSelecteds.Add(cellPos);
            }

            SetPreviewSelection(currentPreviewSelecteds);
        }

        /// <summary>
        /// 드래그 화면 영역과 겹치는 실제 타일 셀들을 공사 대상으로 선택합니다.
        /// </summary>
        protected override void Selects(Camera cam, Vector2 startScreenPos, Vector2 endScreenPos)
        {
            ApplyCommittedSelection(FindSelectableCells(cam, startScreenPos, endScreenPos));
        }

        /// <summary>
        /// 드래그 박스 밖으로 벗어난 타일은 선택 해제하고 새 타일은 선택합니다.
        /// </summary>
        private void ApplyCommittedSelection(HashSet<Vector3Int> currentSelecteds)
        {
            for (int i = selecteds.Count - 1; i >= 0; i--)
            {
                if (currentSelecteds.Contains(selecteds[i]))
                {
                    continue;
                }

                RemoveSelectUI(selecteds[i]);
                selecteds.RemoveAt(i);
            }

            foreach (Vector3Int cellPos in currentSelecteds)
            {
                if (selecteds.Contains(cellPos) == true)
                {
                    continue;
                }

                Add(cellPos);
            }
        }

        /// <summary>
        /// 클릭한 월드 좌표가 포함된 타일 셀을 공사 대상으로 선택합니다.
        /// </summary>
        protected override void Select(Camera cam, Vector2 mouseWorldPosition)
        {
            if (TryGetSelectableCell(mouseWorldPosition, out Vector3Int mouseCellPos) == false) return;

            // 이미 선택된거 아닌지?
            if (selecteds.Contains(mouseCellPos)) return;

            Add(mouseCellPos);
        }

        /// <summary>
        /// 임시 타일 선택 표시를 지우고 확정된 선택 표시를 복원합니다.
        /// </summary>
        protected override void ClearPreview()
        {
            for (int i = 0; i < previewSelecteds.Count; i++)
            {
                RemoveSelectUI(previewSelecteds[i]);
            }

            previewSelecteds.Clear();

            if (isPreviewing == false)
            {
                return;
            }

            isPreviewing = false;
            for (int i = 0; i < selecteds.Count; i++)
            {
                SetSelectUI(selecteds[i], selectedWithBorderTileBase);
            }
        }

        /// <summary>
        /// 선택된 모든 타일 표시를 지우고 선택 목록을 비웁니다.
        /// </summary>
        protected override void Clear()
        {
            for (int i = 0; i < selecteds.Count; i++)
            {
                RemoveSelectUI(selecteds[i]);
            }

            selecteds.Clear();
        }

        /// <summary>
        /// 선택 목록에 셀을 추가하고 선택 표시 타일을 그립니다.
        /// </summary>
        protected override void Add(Vector3Int selectedTargetCellPos)
        {
            selecteds.Add(selectedTargetCellPos);
            SetSelectUI(selectedTargetCellPos, selectedWithBorderTileBase);
        }

        /// <summary>
        /// 현재 선택된 모든 셀에 같은 표시 타일을 적용합니다.
        /// </summary>
        private void SetSelectUI(Vector3Int cellPos, TileBase selectTileBase)
        {
            WorldTilemapController tilemapController = WorldManager.GetWorldTilemapController(WorldTilemapType.WorldTilemapUI);

            if (tilemapController == null) return;

            if (selectTileBase == null) return;

            WorldTile worldTile = new WorldTile
            {
                Pos = cellPos,
                Type = WorldTileType.UI,
                Gravity = 0f,
                TileBase = selectTileBase
            };

            tilemapController.SetTile(worldTile);
        }

        private void RemoveSelectUI(Vector3Int cellPos)
        {
            WorldTilemapController tilemapController = WorldManager.GetWorldTilemapController(WorldTilemapType.WorldTilemapUI);

            if (tilemapController == null) return;

            tilemapController.RemoveTile(cellPos);
        }

        /// <summary>
        /// 드래그 화면 영역에 들어온 선택 가능한 타일 셀을 찾습니다.
        /// </summary>
        private HashSet<Vector3Int> FindSelectableCells(Camera cam, Vector2 startScreenPos, Vector2 endScreenPos)
        {
            HashSet<Vector3Int> currentSelecteds = new();

            // 선택박스 스크린 좌표
            Rect selectionBoxScreenRect = ScreenEx.CreateScreenRect(startScreenPos, endScreenPos);

            // 선택박스 월드 좌표
            Vector3 startWorldPosition = ScreenEx.ScreenToWorldPosition(cam, startScreenPos);
            Vector3 endWorldPosition = ScreenEx.ScreenToWorldPosition(cam, endScreenPos);

            WorldTilemapController tilemapController = WorldManager.GetWorldTilemapController(WorldTilemapType.WorldTilemapGround);
            if (tilemapController == null) return currentSelecteds;

            Tilemap tilemap = tilemapController.Tilemap;
            Vector3Int startCellPos = tilemap.WorldToCell(startWorldPosition);
            Vector3Int endCellPos = tilemap.WorldToCell(endWorldPosition);
            Vector3Int minCellPos = Vector3Int.Min(startCellPos, endCellPos);
            Vector3Int maxCellPos = Vector3Int.Max(startCellPos, endCellPos);

            for (int y = minCellPos.y; y <= maxCellPos.y; y++)
            {
                for (int x = minCellPos.x; x <= maxCellPos.x; x++)
                {
                    Vector3Int cellPos = new Vector3Int(x, y, 0);

                    // 실제 타일이 있는 셀만 선택합니다.
                    if (!tilemap.HasTile(cellPos))
                    {
                        continue;
                    }

                    // 셀 중앙점이 드래그 Rect 안에 들어온 타일만 선택합니다.
                    Vector3 cellCenterWorldPosition = tilemap.GetCellCenterWorld(cellPos);
                    Vector2 cellCenterScreenPos = cam.WorldToScreenPoint(cellCenterWorldPosition);
                    if (!selectionBoxScreenRect.Contains(cellCenterScreenPos))
                    {
                        continue;
                    }

                    currentSelecteds.Add(cellPos);
                }
            }

            return currentSelecteds;
        }

        /// <summary>
        /// 월드 좌표 아래에 선택 가능한 Ground 타일 셀이 있는지 확인합니다.
        /// </summary>
        private bool TryGetSelectableCell(Vector2 mouseWorldPosition, out Vector3Int cellPos)
        {
            cellPos = Vector3Int.zero;

            WorldTilemapController tilemapController = WorldManager.GetWorldTilemapController(WorldTilemapType.WorldTilemapGround);
            if (tilemapController == null) return false;

            Tilemap tilemap = tilemapController.Tilemap;
            cellPos = tilemap.WorldToCell(mouseWorldPosition);

            return tilemap.HasTile(cellPos);
        }

        /// <summary>
        /// 확정 선택은 유지한 채 현재 프레임의 임시 선택 표시만 교체합니다.
        /// </summary>
        private void SetPreviewSelection(HashSet<Vector3Int> currentPreviewSelecteds)
        {
            BeginPreview();

            for (int i = previewSelecteds.Count - 1; i >= 0; i--)
            {
                if (currentPreviewSelecteds.Contains(previewSelecteds[i]) == true)
                {
                    continue;
                }

                RemoveSelectUI(previewSelecteds[i]);
                previewSelecteds.RemoveAt(i);
            }

            foreach (Vector3Int cellPos in currentPreviewSelecteds)
            {
                if (previewSelecteds.Contains(cellPos) == true)
                {
                    continue;
                }

                previewSelecteds.Add(cellPos);
                SetSelectUI(cellPos, selectedWithBorderTileBase);
            }
        }

        /// <summary>
        /// 임시 선택이 시작되면 확정 선택 표시는 잠시 숨깁니다.
        /// </summary>
        private void BeginPreview()
        {
            if (isPreviewing == true)
            {
                return;
            }

            isPreviewing = true;
            for (int i = 0; i < selecteds.Count; i++)
            {
                RemoveSelectUI(selecteds[i]);
            }
        }
    }
}
