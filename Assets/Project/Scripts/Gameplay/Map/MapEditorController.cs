using UnityEngine;
using UnityEngine.Tilemaps;

namespace TRPG.Runtime
{
    /// <summary>
    /// SCN_MapEditor에서 Tilemap을 MapData로 변환하기 위한 씬 설정입니다.
    /// </summary>
    public class MapEditorController : MonoBehaviour
    {
        [Header("Source")]
        [SerializeField] private Tilemap sourceTilemap;

        [Header("Output")]
        [SerializeField] private MapData targetMapData;
        [SerializeField] private string outputFolder = "Assets/Project/Datas/MapController";
        [SerializeField] private string outputAssetName = "SO_MapEditor.asset";

        [Header("Tile Type")]
        [SerializeField] private TileBase groundTile;
        [SerializeField] private TileBase groundSurfaceTile;
        [SerializeField] private MapTileType defaultSolidTileType = MapTileType.Ground;
        [SerializeField] private bool inferSurfaceTile = true;

        [Header("Spawn")]
        [SerializeField] private Transform startSpawnPoint;

        [Header("Coordinate")]
        [SerializeField] private bool normalizePivotToZero = true;

        public Tilemap SourceTilemap => sourceTilemap;

        public MapData TargetMapData => targetMapData;

        public string OutputFolder => outputFolder;

        public string OutputAssetName => outputAssetName;

        public TileBase GroundTile => groundTile;

        public TileBase GroundSurfaceTile => groundSurfaceTile;

        public MapTileType DefaultSolidTileType => defaultSolidTileType;

        public bool InferSurfaceTile => inferSurfaceTile;

        public Transform StartSpawnPoint => startSpawnPoint;

        public bool NormalizePivotToZero => normalizePivotToZero;

        /// <summary>
        /// SourceTilemap이 비어 있으면 자식 Tilemap을 자동으로 연결합니다.
        /// </summary>
        private void OnValidate()
        {
            if (sourceTilemap != null) return;

            sourceTilemap = GetComponentInChildren<Tilemap>();
        }

        /// <summary>
        /// Scene View에서 시작 스폰 위치를 십자 표시로 그립니다.
        /// </summary>
        private void OnDrawGizmos()
        {
            if (startSpawnPoint == null) return;

            // 에디터 씬에서 시작 스폰 위치를 빠르게 식별할 수 있게 표시합니다.
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(startSpawnPoint.position, 0.25f);
            Gizmos.DrawLine(startSpawnPoint.position + Vector3.left * 0.4f, startSpawnPoint.position + Vector3.right * 0.4f);
            Gizmos.DrawLine(startSpawnPoint.position + Vector3.down * 0.4f, startSpawnPoint.position + Vector3.up * 0.4f);
        }
    }
}
