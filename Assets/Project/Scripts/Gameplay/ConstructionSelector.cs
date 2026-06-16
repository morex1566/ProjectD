using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TRPG.Runtime
{
    /// <summary>
    /// 타일 클릭 선택과 드래그 선택 로직을 처리합니다.
    /// </summary>
    public class ConstructionSelector : Selector<Vector3Int>
    {
        [SerializeField] private TileBase selectIndicator;



        protected override void OnEnable()
        {
            base.OnEnable();

            Clear();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            Clear();
        }

        protected override void Selects(Vector2 startScreenPos, Vector2 endScreenPos)
        {
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

                    // 이미 선택된거 아닌지?
                    if (selecteds.Contains(cellPos)) continue;

                    // 셀 중앙점이 드래그 Rect 안에 들어온 타일만 선택합니다.
                    Vector3 cellCenterWorldPos = tilemap.GetCellCenterWorld(cellPos);
                    Vector2 cellCenterScreenPos = cam.WorldToScreenPoint(cellCenterWorldPos);
                    if (!selectionScreenRect.Contains(cellCenterScreenPos)) continue;

                    Add(cellPos);
                }
            }
        }

        protected override void Select(Vector2 mouseWorldPos)
        {
            Tilemap tilemap = WorldManager.Map.Ground;
            Vector3Int mouseCellPos = tilemap.WorldToCell(mouseWorldPos);

            // 실제 타일이 있는 셀만 선택합니다.
            if (!tilemap.HasTile(mouseCellPos)) return;

            // 이미 선택된거 아닌지?
            if (selecteds.Contains(mouseCellPos)) return;

            Add(mouseCellPos);
        }

        protected override void Clear()
        {
            selecteds.Clear();
        }

        protected override void Add(Vector3Int selectedTarget)
        {
            selecteds.Add(selectedTarget);
        }
    }
}
