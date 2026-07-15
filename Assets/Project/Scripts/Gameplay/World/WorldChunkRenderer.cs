using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// WorldChunk의 최종 픽셀 지형 데이터를 청크 텍스처로 표시합니다.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class WorldChunkRenderer : MonoBehaviour
    {
        private WorldChunk chunk;

        private WorldGenerationSettingsData settings;

        private SpriteRenderer spriteRenderer;

        private Texture2D texture;

        private Sprite sprite;


        private void Awake()
        {
            CacheComponents();
        }

        private void OnDestroy()
        {
            ReleaseRenderResources();
        }


        /// <summary>
        /// 청크의 최종 픽셀 지형 데이터를 텍스처로 변환하여 표시합니다.
        /// </summary>
        public void Render(WorldChunk chunk, WorldGenerationSettingsData settings)
        {
            this.chunk = chunk;
            this.settings = settings;

            CacheComponents();
            ReleaseRenderResources();
            CreateRenderResources();
            Refresh();
        }

        /// <summary>
        /// 크기가 고정된 청크 텍스처와 스프라이트를 생성합니다.
        /// </summary>
        private void CreateRenderResources()
        {
            int textureSize = chunk.PixelData.Size;

            texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
            {
                texture.name = $"ChunkTexture_{chunk.Coordinate.x}_{chunk.Coordinate.y}";
                texture.hideFlags = HideFlags.HideAndDontSave;
                texture.filterMode = FilterMode.Point;
                texture.wrapMode = TextureWrapMode.Clamp;
            }

            // 청크 오브젝트의 위치가 텍스처 좌측 하단과 일치하도록 피벗을 좌측 하단에 둡니다.
            sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, textureSize, textureSize),
                Vector2.zero,
                settings.PixelsPerUnit,
                0,
                SpriteMeshType.FullRect);
            {
                sprite.name = $"ChunkSprite_{chunk.Coordinate.x}_{chunk.Coordinate.y}";
                sprite.hideFlags = HideFlags.HideAndDontSave;
            }

            spriteRenderer.sprite = sprite;
        }

        /// <summary>
        /// 현재 청크 픽셀 데이터 전체를 기존 텍스처에 반영합니다.
        /// </summary>
        public void Refresh()
        {
            int textureSize = chunk.PixelData.Size;
            Refresh(new RectInt(0, 0, textureSize, textureSize));
        }

        /// <summary>
        /// 지정한 로컬 픽셀 영역을 기존 청크 텍스처에 반영합니다.
        /// </summary>
        public void Refresh(RectInt dirtyRect)
        {
            Color32[] pixels = CreatePixelData(chunk.PixelData, settings, dirtyRect);

            texture.SetPixels32(
                dirtyRect.x,
                dirtyRect.y,
                dirtyRect.width,
                dirtyRect.height,
                pixels,
                0);

            texture.Apply(false, false);
        }

        /// <summary>
        /// 청크의 지정한 픽셀 영역을 렌더링용 색상 데이터로 변환합니다.
        /// </summary>
        private static Color32[] CreatePixelData(WorldChunkPixelData pixelData, WorldGenerationSettingsData settings, RectInt pixelRect)
        {
            Color32[] pixels = new Color32[pixelRect.width * pixelRect.height];

            for (int y = 0; y < pixelRect.height; y++)
            {
                for (int x = 0; x < pixelRect.width; x++)
                {
                    int localPixelX = pixelRect.x + x;
                    int localPixelY = pixelRect.y + y;
                    int pixelIndex = x + y * pixelRect.width;
                    WorldTileType pixelType = pixelData.GetPixel(localPixelX, localPixelY);

                    pixels[pixelIndex] = GetTileColor(pixelType, settings);
                }
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
        /// SpriteRenderer 컴포넌트를 준비합니다.
        /// </summary>
        private void CacheComponents()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        /// <summary>
        /// 동적으로 생성한 청크 렌더링 리소스를 제거합니다.
        /// </summary>
        private void ReleaseRenderResources()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = null;
            }

            if (sprite != null)
            {
                DestroyGeneratedObject(sprite);
                sprite = null;
            }

            if (texture != null)
            {
                DestroyGeneratedObject(texture);
                texture = null;
            }
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