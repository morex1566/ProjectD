using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    ///<summary>
    /// SpriteRenderer의 불투명 픽셀 영역에 맞춰 BoxCollider2D 크기를 조절합니다.
    ///</summary>
    [DisallowMultipleComponent]
    public class BoxCollider2DSizeFitter : MonoBehaviour
    {
        ///<summary>
        /// 크기를 참조할 SpriteRenderer입니다.
        ///</summary>
        [SerializeField, ReadOnly] private SpriteRenderer spriteRenderer = null;

        ///<summary>
        /// 크기를 조절할 BoxCollider2D입니다.
        ///</summary>
        [SerializeField, ReadOnly] private BoxCollider2D boxCollider = null;

        ///<summary>
        /// 투명 픽셀을 제외한 영역만 사용할지 여부입니다.
        ///</summary>
        [SerializeField] private bool isIgnoreTransparentPixels = true;

        [SerializeField] private bool isFitOnStart = true;

        ///<summary>
        /// 이 알파값보다 큰 픽셀만 불투명 픽셀로 판단합니다.
        ///</summary>
        [SerializeField, Range(0, 255)] private byte alphaThreshold = 1;

        ///<summary>
        /// 콜라이더 크기에 추가할 여백입니다.
        ///</summary>
        [SerializeField] private Vector2 padding = Vector2.zero;

        ///<summary>
        /// Sprite별 불투명 픽셀 영역 캐시입니다.
        ///</summary>
        private static readonly Dictionary<(Sprite Sprite, byte AlphaThreshold), RectInt> opaquePixelRectCache = new();

        ///<summary>
        /// 컴포넌트를 처음 붙이거나 Reset 메뉴를 눌렀을 때 참조를 자동으로 가져옵니다.
        ///</summary>
        private void Reset()
        {
            CacheComponents();
            if (spriteRenderer != null && boxCollider != null)
            {
                Fit();
            }
        }

        private void OnValidate()
        {
            CacheComponents();
            if (spriteRenderer != null && boxCollider != null)
            {
                Fit();
            }
        }

        private void Start()
        {
            CacheComponents();
            if (spriteRenderer != null && boxCollider != null)
            {
                return;
            }

            if (isFitOnStart)
            {
                Fit();
            }
        }

        ///<summary>
        /// Sprite의 불투명 픽셀 영역에 맞춰 BoxCollider2D 크기를 조절합니다.
        ///</summary>
        [ContextMenu(nameof(Fit))]
        public void Fit()
        {
            Sprite sprite = spriteRenderer.sprite;
            if (sprite == null)
            {
                return;
            }

            RectInt pixelRect = GetFullSpritePixelRect(sprite);
            if (isIgnoreTransparentPixels == true)
            {
                TryGetOpaquePixelRect(sprite, out pixelRect);
            }

            ApplyPixelRect(sprite, pixelRect);
        }

        ///<summary>
        /// 필요한 컴포넌트 참조를 가져옵니다.
        ///</summary>
        private void CacheComponents()
        {
            if (boxCollider == null)
            {
                boxCollider = GetComponent<BoxCollider2D>();
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
            }
        }

        ///<summary>
        /// Sprite 전체 픽셀 영역을 가져옵니다.
        ///</summary>
        private RectInt GetFullSpritePixelRect(Sprite sprite)
        {
            return new RectInt(0, 0, Mathf.RoundToInt(sprite.rect.width), Mathf.RoundToInt(sprite.rect.height));
        }

        ///<summary>
        /// Sprite에서 알파값이 있는 픽셀들의 영역을 가져옵니다.
        ///</summary>
        private bool TryGetOpaquePixelRect(Sprite sprite, out RectInt pixelRect)
        {
            (Sprite Sprite, byte AlphaThreshold) cacheKey = (sprite, alphaThreshold);

            if (opaquePixelRectCache.TryGetValue(cacheKey, out pixelRect) == true)
            {
                return true;
            }

            pixelRect = GetFullSpritePixelRect(sprite);

            Texture2D texture = sprite.texture;

            if (texture == null)
            {
                return false;
            }

            if (texture.isReadable == false)
            {
                Debug.LogWarning($"{texture.name} Texture의 Read/Write가 꺼져 있어서 투명 픽셀 계산을 할 수 없습니다.", this);
                return false;
            }

            Rect spriteRect = sprite.rect;

            int textureStartX = Mathf.RoundToInt(spriteRect.x);
            int textureStartY = Mathf.RoundToInt(spriteRect.y);
            int width = Mathf.RoundToInt(spriteRect.width);
            int height = Mathf.RoundToInt(spriteRect.height);

            Color[] pixels = texture.GetPixels(textureStartX, textureStartY, width, height);

            int minX = width;
            int minY = height;
            int maxX = -1;
            int maxY = -1;

            float alphaLimit = alphaThreshold / 255f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // 현재 픽셀의 배열 인덱스를 계산합니다.
                    int index = x + y * width;

                    // 알파값이 기준 이하이면 투명 픽셀로 처리합니다.
                    if (pixels[index].a <= alphaLimit)
                    {
                        continue;
                    }

                    // 불투명 픽셀 영역의 최소/최대 좌표를 갱신합니다.
                    minX = Mathf.Min(minX, x);
                    minY = Mathf.Min(minY, y);
                    maxX = Mathf.Max(maxX, x);
                    maxY = Mathf.Max(maxY, y);
                }
            }

            if (maxX < minX || maxY < minY)
            {
                return false;
            }

            pixelRect = new RectInt(
                minX,
                minY,
                maxX - minX + 1,
                maxY - minY + 1
            );

            opaquePixelRectCache[cacheKey] = pixelRect;

            return true;
        }

        ///<summary>
        /// 픽셀 영역을 BoxCollider2D의 로컬 크기와 오프셋으로 변환해서 적용합니다.
        ///</summary>
        private void ApplyPixelRect(Sprite sprite, RectInt pixelRect)
        {
            float pixelsPerUnit = sprite.pixelsPerUnit;
            Vector2 pivot = sprite.pivot;

            // 픽셀 좌표를 SpriteRenderer 로컬 좌표로 변환합니다.
            Vector2 localMin = ((Vector2)pixelRect.min - pivot) / pixelsPerUnit;
            Vector2 localMax = ((Vector2)pixelRect.max - pivot) / pixelsPerUnit;

            Vector3[] spriteLocalCorners =
            {
                new Vector3(localMin.x, localMin.y, 0f),
                new Vector3(localMin.x, localMax.y, 0f),
                new Vector3(localMax.x, localMin.y, 0f),
                new Vector3(localMax.x, localMax.y, 0f),
            };

            Bounds colliderLocalBounds = new Bounds();
            bool isInitialized = false;

            for (int i = 0; i < spriteLocalCorners.Length; i++)
            {
                // SpriteRenderer 로컬 좌표를 월드 좌표로 변환합니다.
                Vector3 worldPos = spriteRenderer.transform.TransformPoint(spriteLocalCorners[i]);

                // 월드 좌표를 BoxCollider2D 기준 로컬 좌표로 변환합니다.
                Vector3 colliderLocalPos = boxCollider.transform.InverseTransformPoint(worldPos);

                if (isInitialized == false)
                {
                    colliderLocalBounds = new Bounds(colliderLocalPos, Vector3.zero);
                    isInitialized = true;
                    continue;
                }

                colliderLocalBounds.Encapsulate(colliderLocalPos);
            }

            Vector2 finalSize = new Vector2(colliderLocalBounds.size.x + padding.x, colliderLocalBounds.size.y + padding.y);

            // 음수 크기가 되지 않도록 보정합니다.
            finalSize.x = Mathf.Max(0f, finalSize.x);
            finalSize.y = Mathf.Max(0f, finalSize.y);

            boxCollider.size = finalSize;
            boxCollider.offset = colliderLocalBounds.center;
        }
    }
}