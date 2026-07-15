using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 월드 청크 하나의 최종 픽셀 지형 데이터를 관리합니다.
    /// </summary>
    public sealed class WorldChunkPixelData
    {
        private readonly WorldTileType[] pixelTypes;


        public Vector2Int ChunkCoordinate { get; }

        public int Size { get; }


        public WorldChunkPixelData(Vector2Int chunkCoordinate, int size)
        {
            ChunkCoordinate = chunkCoordinate;
            Size = size;
            pixelTypes = new WorldTileType[size * size];
        }

        /// <summary>
        /// 로컬 픽셀 좌표의 지형 종류를 반환합니다.
        /// </summary>
        public WorldTileType GetPixel(int localPixelX, int localPixelY)
        {
            return pixelTypes[ToIndex(localPixelX, localPixelY)];
        }

        /// <summary>
        /// 로컬 픽셀 좌표에 지형 종류를 저장합니다.
        /// </summary>
        public void SetPixel(int localPixelX, int localPixelY, WorldTileType type)
        {
            pixelTypes[ToIndex(localPixelX, localPixelY)] = type;
        }

        /// <summary>
        /// 로컬 픽셀 좌표가 청크 내부인지 확인합니다.
        /// </summary>
        public bool IsInside(int localPixelX, int localPixelY)
        {
            return localPixelX >= 0 && localPixelX < Size &&
                   localPixelY >= 0 && localPixelY < Size;
        }

        /// <summary>
        /// 2차원 로컬 픽셀 좌표를 연속 배열 인덱스로 변환합니다.
        /// </summary>
        private int ToIndex(int localPixelX, int localPixelY)
        {
            return localPixelX + localPixelY * Size;
        }
    }
}