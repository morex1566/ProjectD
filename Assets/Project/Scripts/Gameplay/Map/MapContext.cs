using System;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 맵 타일 타입입니다.
    /// </summary>
    public enum MapTileType
    {
        None,
        Ground,
        GroundSurface,
        Air,
    }

    /// <summary>
    /// 런타임에서 사용하는 맵 상태와 규칙입니다.
    /// </summary>
    [Serializable]
    public class MapContext
    {
        private readonly MapData data;

        public MapData Data => data;

        public int Width => data.Width;

        public int Height => data.Height;

        public float[] Gravities;

        public MapTileType[] TileTypes;

        public event Action<Vector3Int, MapTileType> TileChanged;

        /// <summary>
        /// MapData의 타일과 중력 정보를 런타임 배열로 복사합니다.
        /// </summary>
        public MapContext(MapData data)
        {
            this.data = data;

            Gravities = new float[Width * Height];
            TileTypes = new MapTileType[Width * Height];

            for (int y = 0; y < data.Height; y++)
            {
                for (int x = 0; x < data.Width; x++)
                {
                    int index = ToIndex(x, y);
                    TileTypes[index] = data.GetTileType(x, y);
                    Gravities[index] = WorldManager.Settings.Gravities[TileTypes[index]];
                }
            }
        }

        /// <summary>
        /// 특정 위치의 타일 타입을 반환합니다.
        /// </summary>
        public MapTileType GetTileType(int x, int y)
        {
            if (!IsInBounds(x, y))
            {
                return MapTileType.Air;
            }

            return TileTypes[ToIndex(x, y)];
        }

        /// <summary>
        /// 특정 위치의 중력 값을 반환합니다.
        /// </summary>
        public float GetGravity(int x, int y)
        {
            if (!IsInBounds(x, y))
            {
                return 0f;
            }

            return Gravities[ToIndex(x, y)];
        }

        /// <summary>
        /// 타일을 제거하고 변경 이벤트를 발생시킵니다.
        /// </summary>
        public bool RemoveTile(Vector3Int cellPos)
        {
            if (!IsInBounds(cellPos.x, cellPos.y)) return false;

            int index = ToIndex(cellPos.x, cellPos.y);
            TileTypes[index] = MapTileType.Air;
            Gravities[index] = WorldManager.Settings.Gravities[MapTileType.Air];

            TileChanged?.Invoke(cellPos, MapTileType.Air);
            return true;
        }

        /// <summary>
        /// 지정 좌표가 땅 계열 타일인지 확인합니다.
        /// </summary>
        public bool IsGround(int x, int y)
        {
            MapTileType tileType = GetTileType(x, y);

            return tileType.HasFlag(MapTileType.Ground) ||
                   tileType.HasFlag(MapTileType.GroundSurface);
        }

        /// <summary>
        /// 좌표가 맵 범위 안에 있는지 확인합니다.
        /// </summary>
        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        /// <summary>
        /// 2차원 배열을 1차원으로
        /// </summary>
        public int ToIndex(int x, int y)
        {
            return x + y * Width;
        }
    }
}
