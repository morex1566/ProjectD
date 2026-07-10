using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// Sprite의 불투명 픽셀 정보를 기준으로 발바닥 접점 Transform 위치를 계산합니다.
    /// </summary>
    public class GroundChecker : MonoBehaviour
    {
        [SerializeField, ReadOnly] private SpriteRenderer targetRenderer = null;

        [SerializeField, Range(0f, 1f)] private float alphaThreshold = 0.1f;

        [SerializeField, Min(1)] private int bottomSampleHeight = 1;

        [SerializeField, ReadOnly] private Vector2 spriteLocalPosition = Vector2.zero;

        [SerializeField] private LayerMask groundMask = 0;

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
            hasFootPivot = TryGetFootPivot(targetRenderer, alphaThreshold, bottomSampleHeight, out spriteLocalPosition);
            checkSize = hasFootPivot ? GetPixelWorldSize(targetRenderer) : Vector2.zero;
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
        /// 하위 또는 상위 오브젝트에서 피벗 계산 대상 SpriteRenderer를 찾습니다.
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

            if (targetRenderer == null)
            {
                targetRenderer = transform.parent != null ? transform.parent.GetComponentInChildren<SpriteRenderer>() : null;
            }

            if (groundMask.value == 0)
            {
                groundMask = WorldTilemapType.WorldTilemapGround.ToLayerMask();
            }
        }

        /// <summary>
        /// 발바닥 접점 계산에 사용할 SpriteRenderer를 외부에서 지정합니다.
        /// </summary>
        public void SetTargetRenderer(SpriteRenderer renderer)
        {
            targetRenderer = renderer;
            Init();
        }

        /// <summary>
        /// Sprite의 알파 픽셀 중 가장 아래쪽 영역의 중앙을 Sprite local 좌표로 변환합니다.
        /// </summary>
        private static bool TryGetFootPivot(SpriteRenderer renderer, float alphaThreshold, int bottomSampleHeight, out Vector2 localPosition)
        {
            localPosition = Vector2.zero;

            if (renderer == null || renderer.sprite == null) return false;

            Sprite sprite = renderer.sprite;
            Rect rect = sprite.rect;
            Texture2D texture = sprite.texture;
            if (texture == null) return false;

            Color32[] pixels;
            try
            {
                pixels = texture.GetPixels32();
            }
            catch (UnityException exception)
            {
                Debug.LogWarning($"GroundChecker requires a readable texture. Sprite: {sprite.name}, Error: {exception.Message}", renderer);
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
                    int pixelIndex = (startY + y) * texture.width + startX + x;
                    if (pixels[pixelIndex].a <= alphaLimit) continue;

                    bottomY = y;
                    break;
                }
            }

            if (bottomY < 0) return false;

            int maxSampleY = Mathf.Min(height - 1, bottomY + Mathf.Max(1, bottomSampleHeight) - 1);
            float xSum = 0f;
            int count = 0;

            for (int y = bottomY; y <= maxSampleY; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pixelIndex = (startY + y) * texture.width + startX + x;
                    if (pixels[pixelIndex].a <= alphaLimit) continue;

                    xSum += x + 0.5f;
                    count++;
                }
            }

            if (count <= 0) return false;

            Vector2 pivot = sprite.pivot;
            float localX = (xSum / count - pivot.x) / sprite.pixelsPerUnit;
            float localY = (bottomY + 0.5f - pivot.y) / sprite.pixelsPerUnit;

            if (renderer.flipX) localX = -localX;
            if (renderer.flipY) localY = -localY;

            localPosition = new Vector2(localX, localY);
            return true;
        }

        /// <summary>
        /// Sprite의 1픽셀이 월드에서 차지하는 크기를 계산합니다.
        /// </summary>
        private static Vector2 GetPixelWorldSize(SpriteRenderer renderer)
        {
            if (renderer == null || renderer.sprite == null) return Vector2.zero;
            if (renderer.sprite.pixelsPerUnit <= 0f) return Vector2.zero;

            float pixelLocalSize = 1f / renderer.sprite.pixelsPerUnit;
            Vector3 worldPixelX = renderer.transform.TransformVector(new Vector3(pixelLocalSize, 0f, 0f));
            Vector3 worldPixelY = renderer.transform.TransformVector(new Vector3(0f, pixelLocalSize, 0f));

            return new Vector2(worldPixelX.magnitude, worldPixelY.magnitude);
        }

        private void OnDrawGizmos()
        {
            Vector2 drawSize = checkSize;
            if (drawSize == Vector2.zero) return;

            Gizmos.DrawWireCube(transform.position, new Vector3(drawSize.x, drawSize.y, 0f));

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(
                transform.position + Vector3.left * drawSize.x * 0.5f,
                transform.position + Vector3.right * drawSize.x * 0.5f);
            Gizmos.DrawLine(
                transform.position + Vector3.down * drawSize.y * 0.5f,
                transform.position + Vector3.up * drawSize.y * 0.5f);
        }
    }
}
