using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 새 게임 시작 시 사용할 맵 데이터를 생성합니다.
    /// </summary>
    public static class MapGenerator
    {
        ///// <summary>
        ///// 지정 크기와 지표 높이를 기준으로 평평한 기본 MapData를 생성합니다.
        ///// </summary>
        //public static MapData Generate(int width, int height, int groundHeight, Vector3Int pivot)
        //{
        //    MapData data = ScriptableObject.CreateInstance<MapData>();
        //    data.Init(pivot);
        //    groundHeight = Mathf.Clamp(groundHeight, 1, height);
        //    List<MapTile> tiles = new List<MapTile>(width * height);

        //    for (int y = 0; y < height; y++)
        //    {
        //        for (int x = 0; x < width; x++)
        //        {
        //            tiles.Add(new MapTile
        //            {
        //                Type = GetTileType(y, groundHeight),
        //                Pos = new Vector2Int(x, y)
        //            });
        //        }
        //    }

        //    data.SetTiles(tiles);

        //    return data;
        //}

        ///// <summary>
        ///// 평평한 지형 기준으로 타일 타입을 계산합니다.
        ///// </summary>
        //private static MapTileType GetTileType(int y, int groundHeight)
        //{
        //    if (y >= groundHeight) return MapTileType.Air;

        //    if (y == groundHeight - 1) return MapTileType.GroundSurface;

        //    return MapTileType.Ground;
        //}
    }
}
