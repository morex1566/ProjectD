using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TRPG.Runtime
{
    /// <summary>
    /// WorldChunk의 WorldTile 데이터를 청크 Tilemap에 표시합니다.
    /// </summary>
    [RequireComponent(typeof(Tilemap), typeof(TilemapRenderer))]
    public sealed class WorldChunkRenderer : MonoBehaviour
    {
        private readonly List<Texture2D> textures = new();

        private readonly List<Sprite> sprites = new();

        private readonly List<Tile> tiles = new();

        private readonly Dictionary<WorldTileType, TileBase> tileCache = new();

        private Tilemap tilemap;


        private void Awake()
        {
            CacheComponents();
        }

        private void OnDestroy()
        {
            ReleaseRenderResources();
        }


        /// <summary>
        /// 청크의 각 WorldTile을 픽셀로 그린 뒤 TileBase로 변환하여 표시합니다.
        /// </summary>
        public void Render(WorldChunk chunk, WorldGenerationSettingsData settings)
        {
            CacheComponents();
            ReleaseRenderResources();
            int tilesPerChunk = settings.TilesPerChunk;
            TileBase[] tileBuffer = new TileBase[tilesPerChunk * tilesPerChunk];

            for (int localY = 0; localY < tilesPerChunk; localY++)
            {
                for (int localX = 0; localX < tilesPerChunk; localX++)
                {
                    int tileIndex = localX + localY * tilesPerChunk;
                    WorldTile worldTile = chunk.GetTile(localX, localY);

                    if (worldTile.IsEmpty)
                    {
                        tileBuffer[tileIndex] = null;
                        continue;
                    }

                    tileBuffer[tileIndex] = GetOrCreateTileBase(worldTile, settings);
                }
            }

            BoundsInt chunkBounds = new BoundsInt(
                0,
                0,
                0,
                tilesPerChunk,
                tilesPerChunk,
                1);

            tilemap.SetTilesBlock(chunkBounds, tileBuffer);
        }

        /// <summary>
        /// 같은 WorldTileType은 이미 그려 둔 TileBase를 재사용합니다.
        /// </summary>
        private TileBase GetOrCreateTileBase(WorldTile worldTile, WorldGenerationSettingsData settings)
        {
            if (tileCache.TryGetValue(worldTile.Type, out TileBase tileBase))
            {
                return tileBase;
            }

            tileBase = CreateTileBase(worldTile, settings);
            tileCache.Add(worldTile.Type, tileBase);

            return tileBase;
        }

        /// <summary>
        /// WorldTile을 픽셀 텍스처로 그린 뒤 TileBase로 변환합니다.
        /// </summary>
        private TileBase CreateTileBase(WorldTile worldTile, WorldGenerationSettingsData settings)
        {
            int textureSize = settings.PixelsPerTile;
            Color32[] pixels = CreatePixelData(worldTile, settings, textureSize);

            Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
            {
                texture.hideFlags = HideFlags.HideAndDontSave;
                texture.filterMode = FilterMode.Point;
                texture.wrapMode = TextureWrapMode.Clamp;
                texture.SetPixels32(pixels);
                texture.Apply(false, false);
            }

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, textureSize, textureSize),
                new Vector2(0.5f, 0.5f),
                settings.PixelsPerUnit,
                0,
                SpriteMeshType.FullRect);
            {
                sprite.hideFlags = HideFlags.HideAndDontSave;
            }

            Tile tile = ScriptableObject.CreateInstance<Tile>();
            {
                tile.hideFlags = HideFlags.HideAndDontSave;
                tile.sprite = sprite;
            }

            textures.Add(texture);
            sprites.Add(sprite);
            tiles.Add(tile);

            return tile;
        }

        /// <summary>
        /// WorldTile 하나에 해당하는 픽셀 데이터를 생성합니다.
        /// </summary>
        private static Color32[] CreatePixelData(WorldTile worldTile, WorldGenerationSettingsData settings, int textureSize)
        {
            Color32[] pixels = new Color32[textureSize * textureSize];
            Color32 color = GetTileColor(worldTile.Type, settings);

            for (int pixelIndex = 0; pixelIndex < pixels.Length; pixelIndex++)
            {
                pixels[pixelIndex] = color;
            }

            return pixels;
        }

        /// <summary>
        /// WorldTile 종류에 대응하는 픽셀 색상을 반환합니다.
        /// </summary>
        private static Color32 GetTileColor(WorldTileType tileType, WorldGenerationSettingsData settings)
        {
            switch (tileType)
            {
                case WorldTileType.Soil:
                    return settings.SoilColor;

                case WorldTileType.Stone:
                    return settings.StoneColor;

                default:
                    return new Color32(0, 0, 0, 0);
            }
        }

        /// <summary>
        /// Tilemap 컴포넌트를 준비합니다.
        /// </summary>
        private void CacheComponents()
        {
            if (tilemap == null)
            {
                tilemap = GetComponent<Tilemap>();
            }
        }

        /// <summary>
        /// Tilemap을 비우고 동적으로 생성한 렌더링 리소스를 제거합니다.
        /// </summary>
        private void ReleaseRenderResources()
        {
            if (tilemap != null)
            {
                tilemap.ClearAllTiles();
            }

            foreach (Tile tile in tiles)
            {
                DestroyGeneratedObject(tile);
            }

            foreach (Sprite sprite in sprites)
            {
                DestroyGeneratedObject(sprite);
            }

            foreach (Texture2D texture in textures)
            {
                DestroyGeneratedObject(texture);
            }

            tileCache.Clear();
            tiles.Clear();
            sprites.Clear();
            textures.Clear();
        }

        /// <summary>
        /// 실행 상태에 맞는 방식으로 생성 리소스를 제거합니다.
        /// </summary>
        private static void DestroyGeneratedObject(Object generatedObject)
        {
            if (Application.isPlaying)
            {
                Destroy(generatedObject);
                return;
            }

            DestroyImmediate(generatedObject);
        }
    }
}
