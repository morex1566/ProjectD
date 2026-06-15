using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TRPG.Runtime
{
    /// <summary>
    /// 타일 클릭 선택과 드래그 선택 로직을 처리합니다.
    /// </summary>
    [Serializable]
    public class TileSelector
    {
        [SerializeField] private readonly List<Vector3Int> selectedCells = new();

        [SerializeField] private TileBase selectionTileBase;



        public IReadOnlyList<Vector3Int> SelectedCells => selectedCells;



        public void Selects(Vector2 startScreenPos, Vector2 endScreenPos)
        {
            UnSelect();

            Tilemap tilemap = WorldManager.Map.Ground;
            Camera cam = WorldManager.CamController.Cam;
            Rect selectionScreenRect = ScreenEx.CreateScreenRect(startScreenPos, endScreenPos);

            Vector3 startWorldPos = ScreenEx.ScreenToWorldPos(cam, startScreenPos);
            Vector3 endWorldPos = ScreenEx.ScreenToWorldPos(cam, endScreenPos);

            Vector3Int startCellPos = tilemap.WorldToCell(startWorldPos);
            Vector3Int endCellPos = tilemap.WorldToCell(endWorldPos);

            Vector3Int minCellPos = Vector3Int.Min(startCellPos, endCellPos);
            Vector3Int maxCellPos = Vector3Int.Max(startCellPos, endCellPos);

            for (int y = minCellPos.y; y <= maxCellPos.y; y++)
            {
                for (int x = minCellPos.x; x <= maxCellPos.x; x++)
                {
                    Vector3Int cellPos = new Vector3Int(x, y, 0);

                    // 실제 타일이 있는 셀만 선택합니다.
                    if (!tilemap.HasTile(cellPos)) continue;

                    // 셀 중앙점이 드래그 Rect 안에 들어온 타일만 선택합니다.
                    Vector3 cellCenterWorldPos = tilemap.GetCellCenterWorld(cellPos);
                    Vector2 cellCenterScreenPos = cam.WorldToScreenPoint(cellCenterWorldPos);
                    if (!selectionScreenRect.Contains(cellCenterScreenPos)) continue;

                    selectedCells.Add(cellPos);
                }
            }

            Render(selectionTileBase);
        }

        public void Select(Vector2 mouseWorldPos)
        {
            UnSelect();

            Tilemap tilemap = WorldManager.Map.Ground;
            Vector3Int mouseCellPos = tilemap.WorldToCell(mouseWorldPos);

            // 실제 타일이 있는 셀만 선택합니다.
            if (!tilemap.HasTile(mouseCellPos)) return;

            selectedCells.Add(mouseCellPos);

            Render(selectionTileBase);
        }

        public void UnSelect()
        {
            Render(null);

            selectedCells.Clear();
        }

        public void Render(TileBase tileBase)
        {
            foreach (var cellPos in selectedCells)
            {
                WorldManager.Map.Selection.SetTile(cellPos, tileBase);
            }
        }
    }
}
