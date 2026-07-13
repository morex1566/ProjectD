using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 월드 청크 데이터를 픽셀 텍스처로 표시합니다.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class WorldChunkRenderer : MonoBehaviour
    {
        [SerializeField] private float pixelsPerUnit = 32f;

        private SpriteRenderer spriteRenderer;

        private Texture2D texture;

        private Sprite sprite;


        public float PixelsPerUnit
        {
            get => pixelsPerUnit;
            set => pixelsPerUnit = value;
        }


        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnDestroy()
        {
            ReleaseRenderResources();
        }

        /// <summary>
        /// 청크 데이터를 텍스처와 스프라이트로 변환합니다.
        /// </summary>
        public void Render(WorldChunk chunk)
        {
            ReleaseRenderResources();

            Color32[] pixels = CreatePixelData(chunk);

            texture = new Texture2D(
                WorldChunk.Size,
                WorldChunk.Size,
                TextureFormat.RGBA32,
                false);

            texture.name = $"ChunkTexture_{chunk.Coordinate.x}_{chunk.Coordinate.y}";
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels32(pixels);
            texture.Apply(false, false);

            sprite = Sprite.Create(
                texture,
                new Rect(0, 0, WorldChunk.Size, WorldChunk.Size),
                new Vector2(0.5f, 0.5f),
                pixelsPerUnit,
                0,
                SpriteMeshType.FullRect);

            sprite.name = $"ChunkSprite_{chunk.Coordinate.x}_{chunk.Coordinate.y}";
            spriteRenderer.sprite = sprite;
        }

        /// <summary>
        /// 청크의 각 물질을 화면에 표시할 색으로 변환합니다.
        /// </summary>
        private static Color32[] CreatePixelData(WorldChunk chunk)
        {
            Color32[] pixels = new Color32[WorldChunk.Size * WorldChunk.Size];

            for (int localY = 0; localY < WorldChunk.Size; localY++)
            {
                for (int localX = 0; localX < WorldChunk.Size; localX++)
                {
                    int index = localX + localY * WorldChunk.Size;
                    WorldCell cell = chunk.GetCell(localX, localY);

                    pixels[index] = GetMaterialColor(cell.MaterialType);
                }
            }

            return pixels;
        }

        /// <summary>
        /// 물질 종류에 대응하는 디버그 색상을 반환합니다.
        /// </summary>
        private static Color32 GetMaterialColor(WorldMaterialType materialType)
        {
            switch (materialType)
            {
                case WorldMaterialType.Stone:
                    return new Color32(100, 100, 110, 255);

                case WorldMaterialType.Soil:
                    return new Color32(110, 70, 40, 255);

                case WorldMaterialType.Sand:
                    return new Color32(210, 190, 100, 255);

                case WorldMaterialType.Water:
                    return new Color32(40, 100, 220, 255);

                default:
                    return new Color32(0, 0, 0, 0);
            }
        }

        /// <summary>
        /// 런타임에 생성한 렌더링 리소스를 제거합니다.
        /// </summary>
        private void ReleaseRenderResources()
        {
            spriteRenderer.sprite = null;

            if (sprite != null)
            {
                Destroy(sprite);
                sprite = null;
            }

            if (texture != null)
            {
                Destroy(texture);
                texture = null;
            }
        }
    }
}
