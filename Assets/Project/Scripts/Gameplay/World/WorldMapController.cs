using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TRPG.Runtime
{
    /// <summary>
    /// 맵 런타임 객체와 Unity Tilemap 표현을 연결합니다.
    /// </summary>
    [Serializable]
    public class WorldMapController : MonoBehaviour
    {
        [Header(nameof(WorldMapController))]

        /// <summary>
        /// 맵 생성 시작 셀 위치입니다.
        /// </summary>
        [SerializeField] private Vector3Int pivot = Vector3Int.zero;

        [SerializeField] private WorldMapData data;

        /// <summary>
        /// 런타임 맵 상태입니다.
        /// </summary>
        private WorldMapContext map;

        public WorldMapContext Map => map;

        public WorldMapData MapData => data;

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
        //public WorldTileType TryGetTileType(int x, int y)
        //{
        //    if (map == null)
        //    {
        //        return WorldTileType.Air;
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
        //private void SetMap(WorldMapData mapData)
        //{
        //    if (map != null)
        //    {
        //        map.TileChanged -= OnTileChanged;
        //    }

        //    map = new WorldMapContext(mapData);
        //    map.TileChanged += OnTileChanged;

        //    OnMapGenerated?.Invoke(map.Width * map.Height);
        //}

        ///// <summary>
        ///// 변경된 단일 타일을 Tilemap에 반영합니다.
        ///// </summary>
        //private void OnTileChanged(Vector3Int cellPos, WorldTileType tileType)
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

        //    WorldMapData data = map.Data;
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
        //private TileBase GetTileBase(WorldTileType tileType)
        //{
        //    switch (tileType)
        //    {
        //        case WorldTileType.Ground:
        //            return groundTile;

        //        case WorldTileType.GroundSurface:
        //            return groundSurfaceTile;

        //        case WorldTileType.Air:
        //            return null;

        //        default:
        //            return null;
        //    }
        //}
    }
}
