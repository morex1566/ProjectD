using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 저장 가능한 맵 원본 데이터입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_Map", menuName = "Scriptable Objects/MapController")]
    public class MapData : ScriptableObject
    {
        [SerializeField, Min(1)] private int width = 1;

        [SerializeField, Min(1)] private int height = 1;

        [SerializeField, Min(1)] private int groundHeight = 1;

        [SerializeField] private Vector3Int pivot = Vector3Int.zero;

        [SerializeField] private Vector3Int startSpawnPoint = Vector3Int.zero;

        [SerializeField] private MapTileType[] tileTypes = { MapTileType.Air };

        [SerializeField] private float[] tileGravities = { 0f };

        public int Width => width;

        public int Height => height;

        public int GroundHeight => groundHeight;

        public Vector3Int Pivot => pivot;

        public Vector3Int StartSpawnPoint => startSpawnPoint;

        public float[] Gravities => tileGravities;

        /// <summary>
        /// 런타임 생성 또는 에디터 도구에서 맵 데이터를 초기화합니다.
        /// </summary>
        public void Init(int width, int height, int groundHeight, Vector3Int pivot)
        {
            this.width = Mathf.Max(1, width);
            this.height = Mathf.Max(1, height);
            this.groundHeight = Mathf.Clamp(groundHeight, 1, this.height);
            this.pivot = pivot;
            startSpawnPoint = Vector3Int.zero;

            int tileCount = this.width * this.height;
            tileTypes = new MapTileType[tileCount];
            tileGravities = new float[tileCount];
        }

        /// <summary>
        /// 맵 로컬 좌표 기준의 시작 스폰 위치를 저장합니다.
        /// </summary>
        public void SetStartSpawnPoint(Vector3Int cellPos)
        {
            startSpawnPoint = cellPos;
        }

        /// <summary>
        /// 특정 위치의 타일 타입을 반환합니다.
        /// </summary>
        public MapTileType GetTileType(int x, int y)
        {
            return tileTypes[ToIndex(x, y)];
        }

        /// <summary>
        /// 특정 위치의 타일 타입을 저장합니다.
        /// </summary>
        public void SetTileType(int x, int y, MapTileType tileType)
        {
            tileTypes[ToIndex(x, y)] = tileType;
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

            return tileGravities[ToIndex(x, y)];
        }

        /// <summary>
        /// 특정 위치의 중력 값을 저장합니다.
        /// </summary>
        public void SetGravity(int x, int y, float gravity)
        {
            if (!IsInBounds(x, y)) return;

            tileGravities[ToIndex(x, y)] = gravity;
        }

        /// <summary>
        /// 좌표가 맵 범위 안에 있는지 확인합니다.
        /// </summary>
        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }

        /// <summary>
        /// 2차원 좌표를 1차원 배열 인덱스로 변환합니다.
        /// </summary>
        public int ToIndex(int x, int y)
        {
            return x + y * width;
        }
    }
}
