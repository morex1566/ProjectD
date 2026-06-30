using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TRPG.Runtime
{
    /// <summary>
    /// 새 게임 시작 시 사용할 맵 데이터를 생성합니다.
    /// </summary>
    public class WorldMapEditor : MonoBehaviour
    {
        [SerializeField] private WorldMapData mapData;

        ///// <summary>
        ///// 지정한 셀에 현재 브러시로 타일을 칠합니다.
        ///// </summary>
        //public bool Paint(Vector3Int cellPos)
        //{
        //    if (targetTilemap == null || mapData == null || currentBrush == null)
        //    {
        //        return false;
        //    }

        //    if (!currentBrush.TryGetRandomTile(out TileBase tile))
        //    {
        //        return false;
        //    }

        //    targetTilemap.SetTile(cellPos, tile);

        //    mapData.SetTile(new WorldTile
        //    {
        //        Type = currentBrush.WorldTileType,
        //        Pos = new Vector2Int(cellPos.x, cellPos.y),
        //        Gravity = 0f
        //    });

        //    return true;
        //}

        ///// <summary>
        ///// 지정한 셀의 Tilemap 타일과 저장 데이터를 제거합니다.
        ///// </summary>
        //public bool Erase(Vector3Int cellPos)
        //{
        //    if (targetTilemap == null || mapData == null)
        //    {
        //        return false;
        //    }

        //    targetTilemap.SetTile(cellPos, null);
        //    mapData.RemoveTile(new Vector2Int(cellPos.x, cellPos.y));

        //    return true;
        //}
    }
}
