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

        [Header("Map Setting")]
        [SerializeField, Range(0f, 1f)] private float seaLevel = 0.2f;

        [SerializeField] private int maskLength = 256;

        [SerializeField, Min(1)] private int maskUpscale = 2;

        [SerializeField, Min(1)] private int chunkLength = 128;


        [Header("Preview")]
        [SerializeField] private PreviewMode previewMode = PreviewMode.Land;


        private Map cachedMap = null;


        /// <summary>
        /// 인스펙터 값이 변경될 때 맵 크기와 청크 크기를 보정합니다.
        /// </summary>
        private void OnValidate()
        {
            NormalizeSettings();
        }

        private void NormalizeSettings()
        {
            maskLength = Mathf.Max(1, maskLength);
            maskUpscale = Mathf.Max(1, maskUpscale);
            chunkLength = Mathf.Max(1, chunkLength);

            int mapLength = maskLength * maskUpscale;

            // 최종 맵 길이가 청크 길이로 나누어떨어지지 않으면 기본 청크 길이로 되돌립니다.
            if (mapLength % chunkLength != 0)
            {
                chunkLength = 128;
            }
        }

        [ContextMenu("Generate")]
        public Map Generate()
        {
            NormalizeSettings();

            int mapLength = maskLength * maskUpscale;

            cachedMap = new Map(mapLength, chunkLength);
            GenerateHeightMap(cachedMap);
            GenerateTiles(cachedMap, mapLength);
            GenerateChunks(cachedMap, mapLength);

#if UNITY_EDITOR
            GeneratePreviewMap(previewMode, cachedMap, mapLength);
#endif

            return cachedMap;
        }

        private void GenerateHeightMap(Map map)
        {
            FastNoiseLite noise = new FastNoiseLite(seed);
            noise.SetNoiseType(noiseType);
            noise.SetFrequency(frequency);

            noise.SetFractalType(fractalType);
            noise.SetFractalOctaves(octaves);
            noise.SetFractalGain(gain);
            noise.SetFractalLacunarity(lacunarity);

            for (int y = 0; y < maskLength; y++)
            {
                for (int x = 0; x < maskLength; x++)
                {
                    float heightValue = SampleHeight(noise, x, y);
                    int startX = x * maskUpscale;
                    int startY = y * maskUpscale;

                    // 원본 height 값을 보간하지 않고 같은 값으로 복제해 Map.Heights에 저장합니다.
                    for (int upscaleY = 0; upscaleY < maskUpscale; upscaleY++)
                    {
                        for (int upscaleX = 0; upscaleX < maskUpscale; upscaleX++)
                        {
                            int mapX = startX + upscaleX;
                            int mapY = startY + upscaleY;
                            map.Heights[map.ToIndex(mapX, mapY)] = heightValue;
                        }
                    }
                }
            }
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
            float nx = x / (float)(maskLength - 1) * 2f - 1f;
            float ny = y / (float)(maskLength - 1) * 2f - 1f;

            float distance = Mathf.Sqrt(nx * nx + ny * ny);
            distance = Mathf.Clamp01(distance);

            return Mathf.Pow(distance, falloffPower);
        }

        private Texture GeneratePreviewMap(PreviewMode mode, Map map, int mapLength)
        {
            Texture2D texture = new Texture2D(mapLength, mapLength, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[mapLength * mapLength];

            for (int y = 0; y < mapLength; y++)
            {
                for (int x = 0; x < mapLength; x++)
                {
                    int index = map.ToIndex(x, y);
                    float heightValue = map.Heights[index];
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

                    pixels[index] = color;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            if (previewMap != null)
            {
                previewMap.texture = texture;
            }

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

        private void GenerateTiles(Map map, int mapLength)
        {
            float halfLength = mapLength * 0.5f;

            for (int y = 0; y < mapLength; y++)
            {
                for (int x = 0; x < mapLength; x++)
                {
                    int index = map.ToIndex(x, y);
                    float heightValue = map.Heights[index];

                    map.Tiles[index] = new MapTile
                    {
                        worldPos = new Vector2(x - halfLength, y - halfLength),
                        type = heightValue < seaLevel ? MapTileType.Sea : MapTileType.Land
                    };
                }
            }
        }

        private void GenerateChunks(Map map, int mapLength)
        {
            int chunkCountPerAxis = mapLength / chunkLength;
            int tileCountPerChunk = chunkLength * chunkLength;

            for (int chunkRow = 0; chunkRow < chunkCountPerAxis; chunkRow++)
            {
                for (int chunkX = 0; chunkX < chunkCountPerAxis; chunkX++)
                {
                    int chunkIndex = chunkX + chunkRow * chunkCountPerAxis;
                    int chunkYFromBottom = chunkCountPerAxis - 1 - chunkRow;
                    Chunk chunk = new Chunk
                    {
                        Tiles = new System.Collections.Generic.List<MapTile>(tileCountPerChunk)
                    };

                    FillChunkTiles(map, chunk, chunkX, chunkYFromBottom);
                    map.Chunks[chunkIndex] = chunk;
                }
            }
        }

        private void FillChunkTiles(Map map, Chunk chunk, int chunkX, int chunkY)
        {
            int startX = chunkX * chunkLength;
            int startY = chunkY * chunkLength;

            for (int y = chunkLength - 1; y >= 0; y--)
            {
                for (int x = 0; x < chunkLength; x++)
                {
                    int mapX = startX + x;
                    int mapY = startY + y;

                    chunk.Tiles.Add(map.Tiles[map.ToIndex(mapX, mapY)]);
                }
            }
        }
    }
}
