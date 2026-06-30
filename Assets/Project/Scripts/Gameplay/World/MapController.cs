using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TRPG.Runtime
{
    /// <summary>
    /// 맵 런타임 객체와 Unity Tilemap 표현을 연결합니다.
    /// </summary>
    [Serializable]
    public class MapController : MonoBehaviour
    {
        [Header(nameof(MapController))]

        /// <summary>
        /// 맵 생성 시작 셀 위치입니다.
        /// </summary>
        [SerializeField] private Vector3Int pivot = Vector3Int.zero;

        [SerializeField] private MapData data;

        /// <summary>
        /// 런타임 맵 상태입니다.
        /// </summary>
        private MapContext map;

        public MapContext Map => map;

        public MapData MapData => data;

        public Action<int> OnMapGenerated = null;



        ///// <summary>
        ///// 연결된 MapData가 있으면 로드하고, 없으면 기본 맵을 생성한 뒤 Tilemap에 렌더링합니다.
        ///// </summary>
        //private void Start()
        //{
        //    LoadMap();
        //    RenderMap();
        //}

        ///// <summary>
        ///// 저장되었거나 생성된 맵 데이터를 런타임 맵으로 로드합니다.
        ///// </summary>
        //public void LoadMap()
        //{
        //    if (data == null) return;

        //    SetMap(data);
        //}

        ///// <summary>
        ///// 특정 위치의 타일 타입을 반환합니다.
        ///// </summary>
        //public MapTileType TryGetTileType(int x, int y)
        //{
        //    if (map == null)
        //    {
        //        return MapTileType.Air;
        //    }

        //    return map.TryGetTileType(x, y);
        //}

        ///// <summary>
        ///// 런타임 맵에서 지정 셀의 타일을 제거합니다.
        ///// </summary>
        //public void RemoveTile(Vector3Int cellPos)
        //{
        //    if (map == null) return;
        //    map.RemoveTile(cellPos);
        //}

        ///// <summary>
        ///// MapData를 런타임 Map으로 교체하고 변경 이벤트를 연결합니다.
        ///// </summary>
        //private void SetMap(MapData mapData)
        //{
        //    if (map != null)
        //    {
        //        map.TileChanged -= OnTileChanged;
        //    }

        //    map = new MapContext(mapData);
        //    map.TileChanged += OnTileChanged;

        //    OnMapGenerated?.Invoke(map.Width * map.Height);
        //}

        ///// <summary>
        ///// 변경된 단일 타일을 Tilemap에 반영합니다.
        ///// </summary>
        //private void OnTileChanged(Vector3Int cellPos, MapTileType tileType)
        //{
        //    ground.SetTile(cellPos, GetTileBase(tileType));
        //}

        ///// <summary>
        ///// 맵 데이터를 Tilemap에 반영합니다.
        ///// </summary>
        //private void RenderMap()
        //{
        //    if (map == null)
        //    {
        //        return;
        //    }

        //    ground.ClearAllTiles();

        //    MapData data = map.Data;
        //    BoundsInt bounds = new BoundsInt(data.Pivot.x, data.Pivot.y, 0, map.Width, map.Height, 1);
        //    TileBase[] tileBlock = new TileBase[map.Width * map.Height];

        //    for (int y = 0; y < map.Height; y++)
        //    {
        //        for (int x = 0; x < map.Width; x++)
        //        {
        //            int index = map.ToIndex(x, y);

        //            // 타일 타입에 맞는 TileBase를 넣습니다.
        //            tileBlock[index] = GetTileBase(map.TileTypes[index]);
        //        }
        //    }

        //    // 타일을 한 번에 배치합니다.
        //    ground.SetTilesBlock(bounds, tileBlock);
        //}

        ///// <summary>
        ///// 타일 타입에 맞는 TileBase를 반환합니다.
        ///// </summary>
        //private TileBase GetTileBase(MapTileType tileType)
        //{
        //    switch (tileType)
        //    {
        //        case MapTileType.Ground:
        //            return groundTile;

        //        case MapTileType.GroundSurface:
        //            return groundSurfaceTile;

        //        case MapTileType.Air:
        //            return null;

        //        default:
        //            return null;
        //    }
        //}
    }
}
