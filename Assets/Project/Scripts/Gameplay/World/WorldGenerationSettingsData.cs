using System;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 월드 청크를 텍스처로 표시할 때 사용하는 설정입니다.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "SO_WorldSetup", menuName = "ScriptableObjects/Settings/WorldGenerationSettingsData")]
    public class WorldGenerationSettingsData : ScriptableObject
    {
        [SerializeField] private WorldGenerator worldGenerator = null;

        [SerializeField] private Vector2Int chunkSize = new Vector2Int(5, 5);

        [SerializeField, Min(1)] private int pixelsPerTile = 16;

        [SerializeField, Min(1)] private int tilesPerChunk = 32;

        [SerializeField, Min(1)] private int tilesPerUnit = 1;

        [SerializeField] private Color32 soilColor = new Color32(50, 25, 20, 255);

        [SerializeField] private Color32 soilPatternColor = new Color32(90, 50, 40, 255);

        [SerializeField] private Color32 stoneColor = new Color32(35, 35, 40, 255);

        [SerializeField] private Color32 gravelColor = new Color32(120, 75, 60, 255);

        [SerializeField, Min(0.0001f)] private float soilPatternFrequency = 0.02f;

        [SerializeField, Min(2)] private int soilPatternStepCount = 5;

        [SerializeField, Range(0f, 0.99f)] private float soilPatternThreshold = 0.8f;

        [SerializeField, Min(0.0001f)] private float gravelFrequency = 0.25f;

        [SerializeField, Range(0f, 0.99f)] private float gravelThreshold = 0.85f;

        public int PixelsPerTile => pixelsPerTile;

        public int PixelsPerUnit => pixelsPerTile * tilesPerUnit;

        public int TilesPerChunk => tilesPerChunk;

        public int TilesPerUnit => tilesPerUnit;

        public float TileWorldSize => 1f / tilesPerUnit;

        public float ChunkWorldSize => tilesPerChunk * TileWorldSize;

        public Color32 SoilColor => soilColor;

        public Color32 SoilPatternColor => soilPatternColor;

        public Color32 StoneColor => stoneColor;

        public Color32 GravelColor => gravelColor;

        public float SoilPatternFrequency => soilPatternFrequency;

        public int SoilPatternStepCount => soilPatternStepCount;

        public float SoilPatternThreshold => soilPatternThreshold;

        public float GravelFrequency => gravelFrequency;

        public float GravelThreshold => gravelThreshold;

        public Vector2Int ChunkSize => chunkSize;

        public WorldGenerator WorldGenerator => worldGenerator;
    }
}
