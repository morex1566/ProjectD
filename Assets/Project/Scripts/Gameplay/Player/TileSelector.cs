using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace TRPG.Runtime
{
    /// <summary>
    /// 타일 클릭 선택과 드래그 선택 로직을 처리합니다.
    /// </summary>
    public class TileSelector : Selector<Vector3Int>
    {
        [SerializeField] private TileBase selectedWithBorderTileBase;

        [SerializeField] private TileBase selectedWithoutBorderTileBase;





        public TileBase TileSelectedIndicator => selectedWithBorderTileBase;



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
        /// 드래그 화면 영역과 겹치는 실제 타일 셀들을 공사 대상으로 선택합니다.
        /// </summary>
        protected override void Selects(Camera cam, Vector2 startScreenPos, Vector2 endScreenPos)
        {
            //Tilemap tilemap = WorldManager.WorldGridController.WorldTilemapGround;
            //Camera cam = WorldManager.CamController.Cam;
            //Rect selectionScreenRect = ScreenEx.CreateScreenRect(startScreenPos, endScreenPos);

            //Vector3 startWorldPos = ScreenEx.ScreenToWorldPos(cam, startScreenPos);
            //Vector3 endWorldPos = ScreenEx.ScreenToWorldPos(cam, endScreenPos);

            //Vector3Int startCellPos = tilemap.WorldToCell(startWorldPos);
            //Vector3Int endCellPos = tilemap.WorldToCell(endWorldPos);

            //Vector3Int minCellPos = Vector3Int.Min(startCellPos, endCellPos);
            //Vector3Int maxCellPos = Vector3Int.Max(startCellPos, endCellPos);

            //for (int y = minCellPos.y; y <= maxCellPos.y; y++)
            //{
            //    for (int x = minCellPos.x; x <= maxCellPos.x; x++)
            //    {
            //        Vector3Int cellPos = new Vector3Int(x, y, 0);

            //        // 실제 타일이 있는 셀만 선택합니다.
            //        if (!tilemap.HasTile(cellPos)) continue;

            //        // 이미 선택된거 아닌지?
            //        if (selecteds.Contains(cellPos)) continue;

            //        // 셀 중앙점이 드래그 Rect 안에 들어온 타일만 선택합니다.
            //        Vector3 cellCenterWorldPos = tilemap.GetCellCenterWorld(cellPos);
            //        Vector2 cellCenterScreenPos = cam.WorldToScreenPoint(cellCenterWorldPos);
            //        if (!selectionScreenRect.Contains(cellCenterScreenPos)) continue;

            //        Add(cellPos);
            //    }
            //}
        }

        /// <summary>
        /// 클릭한 월드 좌표가 포함된 타일 셀을 공사 대상으로 선택합니다.
        /// </summary>
        protected override void Select(Camera cam, Vector2 mouseWorldPos)
        {
            //Tilemap tilemap = WorldManager.WorldGridController.WorldTilemapGround;
            //Vector3Int mouseCellPos = tilemap.WorldToCell(mouseWorldPos);

            //// 실제 타일이 있는 셀만 선택합니다.
            //if (!tilemap.HasTile(mouseCellPos)) return;

            //// 이미 선택된거 아닌지?
            //if (selecteds.Contains(mouseCellPos)) return;

            //Add(mouseCellPos);
        }

        /// <summary>
        /// 선택된 모든 타일 표시를 지우고 선택 목록을 비웁니다.
        /// </summary>
        protected override void Clear()
        {
            ShowIndicators(null);
            selecteds.Clear();
        }

        /// <summary>
        /// 선택 목록에 셀을 추가하고 선택 표시 타일을 그립니다.
        /// </summary>
        protected override void Add(Vector3Int selectedTarget)
        {
            selecteds.Add(selectedTarget);
            ShowIndicator(selectedTarget, selectedWithBorderTileBase);
        }

        /// <summary>
        /// 현재 선택된 모든 셀에 같은 표시 타일을 적용합니다.
        /// </summary>
        private void ShowIndicators(TileBase indicator)
        {
            for (int i = 0; i < selecteds.Count; i++)
            {
                ShowIndicator(selecteds[i], indicator);
            }
        }

        /// <summary>
        /// WorldTilemapUI Tilemap의 단일 셀 표시를 설정합니다.
        /// </summary>
        private void ShowIndicator(Vector3Int cellPos, TileBase indicator)
        {
            //if (WorldManager.WorldGridController.WorldTilemapUI == null) return;

            //WorldManager.WorldGridController.WorldTilemapUI.SetTile(cellPos, indicator);
        }
    }
}
