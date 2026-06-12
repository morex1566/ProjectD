using UnityEngine;
using UnityEngine.UI;

namespace TRPG.Runtime
{
    public class MapGenerator : MonoBehaviour
    {
        private enum PreviewMode
        {
            Height,
            Land
        }

        [Header("Output")]
        [SerializeField] private RawImage previewMap;

        [Header("Map Size")]
        [SerializeField] private int width = 256;
        [SerializeField] private int height = 256;

        [Header("FastNoiseLite")]
        [SerializeField] private int seed = 4;
        [SerializeField] private FastNoiseLite.NoiseType noiseType = FastNoiseLite.NoiseType.OpenSimplex2;
        [SerializeField, Range(0.001f, 0.1f)] private float frequency = 0.01f;

        [Header("Fractal")]
        [SerializeField] private FastNoiseLite.FractalType fractalType = FastNoiseLite.FractalType.FBm;
        [SerializeField, Range(1, 10)] private int octaves = 4;
        [SerializeField, Range(0f, 1f)] private float gain = 0.5f;
        [SerializeField] private float lacunarity = 2f;

        [Header("Offset")]
        [SerializeField] private Vector2 offset;

        [Header("Island Falloff")]
        [SerializeField] private bool useIslandFalloff = true;
        [SerializeField, Range(0f, 2f)] private float falloffStrength = 1f;
        [SerializeField, Range(0.1f, 5f)] private float falloffPower = 3f;

        [Header("Chunk")]

        /// <summary>
        /// MapTexture를 가로/세로 몇 개의 청크로 나눌 것인지.
        /// 예: 16이면 전체 맵은 16 x 16 청크.
        /// </summary>
        [SerializeField] private int chunkSize = 16;

        /// <summary>
        /// 청크 하나에 들어가는 총 타일 개수.
        /// 예: 256이면 1청크는 16 x 16 타일.
        /// </summary>
        [SerializeField] private int tileCountPerChunk = 256;

        [Header("Map Setting")]
        [SerializeField, Range(0f, 1f)] private float seaLevel = 0.2f;

        [Header("Preview")]
        [SerializeField] private PreviewMode previewMode = PreviewMode.Land;

        [SerializeField] private bool autoUpdate = true;


        private Map cachedMap = null;


        public int ChunkTileSize => Mathf.RoundToInt(Mathf.Sqrt(tileCountPerChunk));

        public int MapWidth => chunkSize * ChunkTileSize;

        public int MapHeight => chunkSize * ChunkTileSize;



        private void OnValidate()
        {
            chunkSize = Mathf.Max(1, chunkSize);
            tileCountPerChunk = Mathf.Max(1, tileCountPerChunk);
            frequency = Mathf.Max(0.001f, frequency);
            octaves = Mathf.Max(1, octaves);
            lacunarity = Mathf.Max(1f, lacunarity);

            if (autoUpdate) GenerateMap();
        }

        [ContextMenu("GenerateMap")]
        public Map GenerateMap()
        {
            cachedMap = new()
            { 
                heights = GenerateHeightMap()
            };

            return cachedMap;
        }

        [ContextMenu("GeneratePreviewMap")]
        public Texture GeneratePreviewMap()
        {
            if (previewMap == null) return null;

            previewMap.texture = GeneratePreviewMap(previewMode, cachedMap);

            return previewMap.texture;
        }

        private float[,] GenerateHeightMap()
        {
            float[,] heightMap = new float[width, height];

            FastNoiseLite noise = new FastNoiseLite(seed);
            noise.SetNoiseType(noiseType);
            noise.SetFrequency(frequency);

            noise.SetFractalType(fractalType);
            noise.SetFractalOctaves(octaves);
            noise.SetFractalGain(gain);
            noise.SetFractalLacunarity(lacunarity);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float heightValue = SampleHeight(noise, x, y);
                    heightMap[x, y] = heightValue;
                }
            }

            return heightMap;
        }

        private float SampleHeight(FastNoiseLite noise, int x, int y)
        {
            float sampleX = x + offset.x;
            float sampleY = y + offset.y;

            float value = noise.GetNoise(sampleX, sampleY);

            // FastNoiseLite의 기본 출력은 -1 ~ 1이므로 0 ~ 1로 변환.
            value = value * 0.5f + 0.5f;

            if (useIslandFalloff)
            {
                float falloff = GetIslandFalloff(x, y);
                value -= falloff * falloffStrength;
            }

            return Mathf.Clamp01(value);
        }

        private float GetIslandFalloff(int x, int y)
        {
            float nx = x / (float)(width - 1) * 2f - 1f;
            float ny = y / (float)(height - 1) * 2f - 1f;

            float distance = Mathf.Sqrt(nx * nx + ny * ny);
            distance = Mathf.Clamp01(distance);

            return Mathf.Pow(distance, falloffPower);
        }

        private Texture GeneratePreviewMap(PreviewMode mode, Map map)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float heightValue = map.heights[x, y];
                    Color color;

                    switch (mode)
                    {
                        case PreviewMode.Height:
                            color = GetHeightColor(heightValue);
                            break;

                        case PreviewMode.Land:
                            color = GetLandColor(heightValue);
                            break;

                        default:
                            color = GetLandColor(heightValue);
                            break;
                    }

                    pixels[y * width + x] = color;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return texture;
        }

        private Color GetHeightColor(float heightValue)
        {
            return new Color(heightValue, heightValue, heightValue, 1f);
        }

        private Color GetLandColor(float heightValue)
        {
            if (heightValue < seaLevel) return Color.black;

            return Color.white;
        }



        public MapTileType[,] GenerateTileMap(float[,] heightMap)
        {
            MapTileType[,] tileMap = new MapTileType[MapWidth, MapHeight];

            for (int y = 0; y < MapHeight; y++)
            {
                for (int x = 0; x < MapWidth; x++)
                {
                    // seaLevel보다 낮으면 바다, 높으면 땅으로 판정한다.
                    tileMap[x, y] = heightMap[x, y] < seaLevel
                        ? MapTileType.Sea
                        : MapTileType.Land;
                }
            }

            return tileMap;
        }

        public void GenerateChunk(MapTileType[,] tileMap, int chunkX, int chunkY)
        {
            int startX = chunkX * ChunkTileSize;
            int startY = chunkY * ChunkTileSize;

            for (int localY = 0; localY < ChunkTileSize; localY++)
            {
                for (int localX = 0; localX < ChunkTileSize; localX++)
                {
                    int tileX = startX + localX;
                    int tileY = startY + localY;

                    MapTileType tileType = tileMap[tileX, tileY];

                    // 다음 단계에서 여기서 실제 타일을 생성하거나 Tilemap에 배치한다.
                }
            }
        }
    }
}
