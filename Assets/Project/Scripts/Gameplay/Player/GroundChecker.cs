using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// Sprite의 불투명 픽셀을 기준으로 발바닥 접점 위치를 계산합니다.
    /// </summary>
    public sealed class GroundChecker : MonoBehaviour
    {
        [SerializeField, ReadOnly] private SpriteRenderer targetRenderer = null;

        [SerializeField, Range(0f, 1f)] private float alphaThreshold = 0.1f;

        [SerializeField, Min(1)] private int bottomSampleHeight = 1;

        [SerializeField, ReadOnly] private Vector2 spriteLocalPosition = Vector2.zero;

        [SerializeField, ReadOnly] private Vector2 checkSize = Vector2.zero;

        [SerializeField, ReadOnly] private bool hasFootPivot = false;


        public SpriteRenderer TargetRenderer => targetRenderer;

        public Vector2 SpriteLocalPosition => spriteLocalPosition;

        public Vector2 CheckSize => checkSize;

        public bool HasFootPivot => hasFootPivot;


        private void OnValidate()
        {
            Init();
        }

        private void Awake()
        {
            Init();
        }

        private void Start()
        {
            Generate();
        }

        /// <summary>
        /// 현재 SpriteRenderer와 Sprite 상태를 기준으로 발바닥 접점 위치를 다시 계산합니다.
        /// </summary>
        [ContextMenu("Generate Foot")]
        public void Generate()
        {
            Init();

            hasFootPivot = TryGetFootPivot(targetRenderer, alphaThreshold, bottomSampleHeight, out spriteLocalPosition);

            if (hasFootPivot == false && targetRenderer != null && targetRenderer.sprite != null)
            {
                Bounds spriteBounds = targetRenderer.sprite.bounds;
                spriteLocalPosition = new Vector2(spriteBounds.center.x, spriteBounds.min.y);
                hasFootPivot = true;
            }

            checkSize = hasFootPivot == true ? GetPixelWorldSize(targetRenderer) : Vector2.zero;
            if (hasFootPivot == false)
            {
                return;
            }

            Vector3 worldPosition = targetRenderer.transform.TransformPoint(spriteLocalPosition);
            worldPosition += Vector3.down * (checkSize.y * 0.5f);

            if (transform.parent == null)
            {
                transform.position = worldPosition;
                return;
            }

            Vector3 localPosition = transform.parent.InverseTransformPoint(worldPosition);
            localPosition.z = transform.localPosition.z;
            transform.localPosition = localPosition;
        }

        /// <summary>
        /// 하위 또는 상위 오브젝트에서 계산 대상 SpriteRenderer를 찾습니다.
        /// </summary>
        public void Init()
        {
            if (targetRenderer == null)
            {
                targetRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (targetRenderer == null)
            {
                targetRenderer = GetComponentInParent<SpriteRenderer>();
            }

            if (targetRenderer == null && transform.parent != null)
            {
                targetRenderer = transform.parent.GetComponentInChildren<SpriteRenderer>();
            }
        }

        /// <summary>
        /// 발바닥 접점 계산에 사용할 SpriteRenderer를 지정합니다.
        /// </summary>
        public void SetTargetRenderer(SpriteRenderer renderer)
        {
            targetRenderer = renderer;
            Init();
        }

        /// <summary>
        /// Sprite의 가장 아래쪽 불투명 픽셀 중심을 로컬 좌표로 반환합니다.
        /// </summary>
        private static bool TryGetFootPivot(SpriteRenderer renderer, float alphaThreshold, int bottomSampleHeight, out Vector2 localPosition)
        {
            localPosition = Vector2.zero;

            if (renderer == null || renderer.sprite == null || renderer.sprite.texture == null)
            {
                return false;
            }

            Sprite sprite = renderer.sprite;
            Rect rect = sprite.rect;
            Color32[] pixels;

            try
            {
                pixels = sprite.texture.GetPixels32();
            }
            catch (UnityException)
            {
                return false;
            }

            int startX = Mathf.RoundToInt(rect.x);
            int startY = Mathf.RoundToInt(rect.y);
            int width = Mathf.RoundToInt(rect.width);
            int height = Mathf.RoundToInt(rect.height);
            byte alphaLimit = (byte)Mathf.Clamp(Mathf.RoundToInt(alphaThreshold * byte.MaxValue), 0, byte.MaxValue);

            int bottomY = -1;
            for (int y = 0; y < height && bottomY < 0; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pixelIndex = (startY + y) * sprite.texture.width + startX + x;
                    if (pixels[pixelIndex].a <= alphaLimit)
                    {
                        continue;
                    }

                    bottomY = y;
                    break;
                }
            }

            if (bottomY < 0)
            {
                return false;
            }

            int maximumSampleY = Mathf.Min(height - 1, bottomY + Mathf.Max(1, bottomSampleHeight) - 1);
            float xSum = 0f;
            int count = 0;

            for (int y = bottomY; y <= maximumSampleY; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pixelIndex = (startY + y) * sprite.texture.width + startX + x;
                    if (pixels[pixelIndex].a <= alphaLimit)
                    {
                        continue;
                    }

                    xSum += x + 0.5f;
                    count++;
                }
            }

            if (count <= 0)
            {
                return false;
            }

            Vector2 pivot = sprite.pivot;
            float localX = (xSum / count - pivot.x) / sprite.pixelsPerUnit;
            float localY = (bottomY + 0.5f - pivot.y) / sprite.pixelsPerUnit;

            if (renderer.flipX == true)
            {
                localX = -localX;
            }

            if (renderer.flipY == true)
            {
                localY = -localY;
            }

            localPosition = new Vector2(localX, localY);
            return true;
        }

        /// <summary>
        /// Sprite 한 픽셀이 월드에서 차지하는 크기를 반환합니다.
        /// </summary>
        private static Vector2 GetPixelWorldSize(SpriteRenderer renderer)
        {
            if (renderer == null || renderer.sprite == null || renderer.sprite.pixelsPerUnit <= 0f)
            {
                return Vector2.zero;
            }

            float pixelLocalSize = 1f / renderer.sprite.pixelsPerUnit;
            Vector3 worldPixelX = renderer.transform.TransformVector(new Vector3(pixelLocalSize, 0f, 0f));
            Vector3 worldPixelY = renderer.transform.TransformVector(new Vector3(0f, pixelLocalSize, 0f));

            return new Vector2(worldPixelX.magnitude, worldPixelY.magnitude);
        }
    }
}
