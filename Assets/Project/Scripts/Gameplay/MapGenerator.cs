using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TRPG.Runtime
{
    /// <summary>
    /// 맵 타일 타입입니다.
    /// </summary>
    [Flags]
    public enum MapTileType
    {
        None = 0,
        Ground = 1 << 0,
        GroundSurface = 1 << 1,
        Air = 1 << 2,
    }

    /// <summary>
    /// 완전히 평평한 테라리아식 2D 지형을 생성합니다.
    /// </summary>
    public class MapGenerator : MonoBehaviour
    {
        /// <summary>
        /// 바닥 타일맵입니다.
        /// </summary>
        [SerializeField] private Tilemap ground;

        /// <summary>
        /// 일반 지하 흙 타일입니다.
        /// </summary>
        [SerializeField] private TileBase groundTile;

        /// <summary>
        /// 지표면 흙 타일입니다.
        /// </summary>
        [SerializeField] private TileBase groundSurfaceTile;

        /// <summary>
        /// 맵의 가로 타일 개수입니다.
        /// </summary>
        [SerializeField, Min(1)] private int mapWidth = 256;

        /// <summary>
        /// 맵의 세로 타일 개수입니다.
        /// </summary>
        [SerializeField, Min(1)] private int mapHeight = 128;

        /// <summary>
        /// 땅이 차지하는 높이입니다.
        /// </summary>
        [SerializeField, Min(1)] private int groundHeight = 64;

        /// <summary>
        /// 맵 생성 시작 셀 위치입니다.
        /// </summary>
        [SerializeField] private Vector3Int pivot = Vector3Int.zero;

        /// <summary>
        /// 맵 타일 데이터입니다.
        /// </summary>
        private MapTileType[] tiles;


        public Tilemap Ground => ground;

        public Action<int> OnMapGenerated = null;

        public int MapWidth => mapWidth;

        public int MapHeight => mapHeight;

        public Vector2Int Center => new Vector2Int(MapWidth / 2, MapHeight / 2);
        

        /// <summary>
        /// 시작 시 맵을 생성합니다.
        /// </summary>
        private void Start()
        {
            Generate();
        }

        /// <summary>
        /// 평평한 지형을 생성합니다.
        /// </summary>
        [ContextMenu("Generate")]
        public void Generate()
        {
            if (ground == null || groundTile == null || groundSurfaceTile == null)
            {
                return;
            }

            // 땅 높이가 맵 높이를 넘지 않게 제한합니다.
            groundHeight = Mathf.Clamp(groundHeight, 1, mapHeight);

            // 기존 타일맵을 초기화합니다.
            ground.ClearAllTiles();

            // 맵 데이터를 생성합니다.
            tiles = new MapTileType[mapWidth * mapHeight];

            GenerateTileData();
            RenderTilemap();
        }

        /// <summary>
        /// Ground / GroundSurface / Air 데이터를 생성합니다.
        /// </summary>
        private void GenerateTileData()
        {
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    int index = ToIndex(x, y);

                    if (y >= groundHeight)
                    {
                        // 땅 높이보다 위쪽은 공기입니다.
                        tiles[index] = MapTileType.Air;
                    }
                    else if (y == groundHeight - 1)
                    {
                        // 땅의 가장 윗줄은 지표면입니다.
                        tiles[index] = MapTileType.GroundSurface;
                    }
                    else
                    {
                        // 지표면 아래는 일반 땅입니다.
                        tiles[index] = MapTileType.Ground;
                    }
                }
            }
        }

        /// <summary>
        /// 맵 데이터를 Tilemap에 반영합니다.
        /// </summary>
        private void RenderTilemap()
        {
            BoundsInt bounds = new BoundsInt(pivot.x, pivot.y, 0, mapWidth, mapHeight, 1);
            TileBase[] tileBlock = new TileBase[mapWidth * mapHeight];

            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    int index = ToIndex(x, y);

                    // 타일 타입에 맞는 TileBase를 넣습니다.
                    tileBlock[index] = GetTileBase(tiles[index]);
                }
            }

            // 타일을 한 번에 배치합니다.
            ground.SetTilesBlock(bounds, tileBlock);
        }

        /// <summary>
        /// 특정 위치의 타일 타입을 반환합니다.
        /// </summary>
        public MapTileType GetTileType(int x, int y)
        {
            if (IsInBounds(x, y) == false)
            {
                return MapTileType.Air;
            }

            return tiles[ToIndex(x, y)];
        }

        /// <summary>
        /// 타일 타입에 맞는 TileBase를 반환합니다.
        /// </summary>
        private TileBase GetTileBase(MapTileType tileType)
        {
            switch (tileType)
            {
                case MapTileType.Ground:
                    return groundTile;

                case MapTileType.GroundSurface:
                    return groundSurfaceTile;

                case MapTileType.Air:
                    return null;

                default:
                    return null;
            }
        }

        /// <summary>
        /// 로컬 맵 좌표를 Tilemap 셀 좌표로 변환합니다.
        /// </summary>
        private Vector3Int ToCellPos(int x, int y)
        {
            return new Vector3Int(pivot.x + x, pivot.y + y, pivot.z);
        }

        /// <summary>
        /// 좌표가 맵 범위 안에 있는지 확인합니다.
        /// </summary>
        private bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < mapWidth && y >= 0 && y < mapHeight;
        }

        /// <summary>
        /// 2차원 좌표를 1차원 배열 인덱스로 변환합니다.
        /// </summary>
        private int ToIndex(int x, int y)
        {
            return x + y * mapWidth;
        }
    }
}