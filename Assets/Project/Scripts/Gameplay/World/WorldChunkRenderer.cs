using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 타일 데이터를 청크 텍스처 한 장으로 변환합니다.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class WorldChunkRenderer : MonoBehaviour
    {
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
        /// 월드 청크를 텍스처와 스프라이트로 변환합니다.
        /// </summary>
        public void Render(WorldChunk chunk, WorldSetup setup)
        {
            setup.Validate();

            CacheComponents();
            ReleaseRenderResources();

            int textureSize = WorldChunk.Size * setup.PixelsPerTile;
            Color32[] pixels = CreatePixelData(chunk, setup, textureSize);

            texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
            {
                texture.name = $"ChunkTexture_{chunk.Coordinate.x}_{chunk.Coordinate.y}";
                texture.hideFlags = HideFlags.HideAndDontSave;
                texture.filterMode = FilterMode.Point;
                texture.wrapMode = TextureWrapMode.Clamp;
                texture.SetPixels32(pixels);
                texture.Apply(false, false);
            }

            sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, textureSize, textureSize),
                new Vector2(0.5f, 0.5f),
                setup.PixelsPerUnit,
                0,
                SpriteMeshType.FullRect);
            {
                sprite.name = $"ChunkSprite_{chunk.Coordinate.x}_{chunk.Coordinate.y}";
                sprite.hideFlags = HideFlags.HideAndDontSave;
            }

            spriteRenderer.sprite = sprite;
        }


        /// <summary>
        /// 청크의 모든 타일을 픽셀 배열로 변환합니다.
        /// </summary>
        private static Color32[] CreatePixelData(WorldChunk chunk, WorldSetup setup, int textureSize)
        {
            Color32[] pixels = new Color32[textureSize * textureSize];

            for (int tileY = 0; tileY < WorldChunk.Size; tileY++)
            {
                for (int tileX = 0; tileX < WorldChunk.Size; tileX++)
                {
                    WorldTile tile = chunk.GetTile(tileX, tileY);
                    Color32 tileColor = GetTileColor(tile.Type, setup);

                    PaintTile(pixels, textureSize, tileX, tileY, setup.PixelsPerTile, tileColor);
                }
            }

            return pixels;
        }

        /// <summary>
        /// 타일 하나에 해당하는 픽셀 영역을 같은 색으로 채웁니다.
        /// </summary>
        private static void PaintTile(Color32[] pixels, int textureSize, int tileX, int tileY, int pixelsPerTile, Color32 color)
        {
            int startX = tileX * pixelsPerTile;
            int startY = tileY * pixelsPerTile;

            for (int pixelY = 0; pixelY < pixelsPerTile; pixelY++)
            {
                for (int pixelX = 0; pixelX < pixelsPerTile; pixelX++)
                {
                    int textureX = startX + pixelX;
                    int textureY = startY + pixelY;
                    int pixelIndex = textureX + textureY * textureSize;

                    pixels[pixelIndex] = color;
                }
            }
        }

        /// <summary>
        /// 타일 종류에 대응하는 기본 표시 색상을 반환합니다.
        /// </summary>
        private static Color32 GetTileColor(WorldTileType tileType, WorldSetup setup)
        {
            switch (tileType)
            {
                case WorldTileType.Soil:
                    return setup.SoilColor;

                case WorldTileType.Stone:
                    return setup.StoneColor;

                default:
                    return new Color32(0, 0, 0, 0);
            }
        }

        /// <summary>
        /// 필요한 Unity 컴포넌트를 준비합니다.
        /// </summary>
        private void CacheComponents()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        /// <summary>
        /// 런타임 또는 에디터에서 생성한 렌더링 리소스를 제거합니다.
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