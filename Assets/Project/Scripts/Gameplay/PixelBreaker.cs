using UnityEngine;

namespace TRPG.Runtime
{
    public class PixelBreaker : MonoBehaviour
    {
        [Header(nameof(PixelBreaker))]
        [SerializeField] private SpriteRenderer targetSpriteRenderer;
        [SerializeField] private GameObject pixelPiecePb;

        [Header("Pixel Piece")]
        [SerializeField] private int sampleStep = 2;

        [Header("Explosion")]
        [SerializeField] private float explosionForce = 4f;
        [SerializeField] private float randomForce = 0.5f;
        [SerializeField] private float upwardForce = 1.2f;
        [SerializeField] private float angularForce = 360f;
        [SerializeField] private float lifeTime = 2f;

        public void Break(Vector2 hitDirection)
        {
            Sprite sprite = targetSpriteRenderer.sprite;
            Texture2D texture = sprite.texture;
            Rect textureRect = sprite.textureRect;

            hitDirection = hitDirection.normalized;

            for (int y = 0; y < textureRect.height; y += sampleStep)
            {
                for (int x = 0; x < textureRect.width; x += sampleStep)
                {
                    if (!TrySampleBlockColor(texture, textureRect, x, y, out Color pixelColor)) continue;

                    Vector3 worldPos = GetPixelWorldPos(sprite, x, y);

                    SpawnPixelPiece(worldPos, pixelColor, hitDirection);
                }
            }

            targetSpriteRenderer.enabled = false;
        }

        private bool TrySampleBlockColor(Texture2D texture, Rect textureRect, int startX, int startY, out Color color)
        {
            Color weightedColorSum = Color.clear;
            float alphaSum = 0f;
            int validPixelCount = 0;

            int endX = Mathf.Min(startX + sampleStep, (int)textureRect.width);
            int endY = Mathf.Min(startY + sampleStep, (int)textureRect.height);

            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    int textureX = (int)textureRect.x + x;
                    int textureY = (int)textureRect.y + y;

                    Color pixelColor = texture.GetPixel(textureX, textureY);

                    // 투명 픽셀은 빈 공간으로 보고 조각 색상 평균에서 제외합니다.
                    if (pixelColor.a <= 0.1f) continue;

                    weightedColorSum += pixelColor * pixelColor.a;
                    alphaSum += pixelColor.a;
                    validPixelCount++;
                }
            }

            if (validPixelCount == 0)
            {
                color = Color.clear;
                return false;
            }

            color = weightedColorSum / alphaSum;
            color.a = Mathf.Clamp01(alphaSum / validPixelCount);

            return true;
        }

        private Vector3 GetPixelWorldPos(Sprite sprite, int x, int y)
        {
            Vector2 pivot = sprite.pivot;

            Vector2 localPos = new Vector2
            (
                (x - pivot.x) / sprite.pixelsPerUnit,
                (y - pivot.y) / sprite.pixelsPerUnit
            );

            return targetSpriteRenderer.transform.TransformPoint(localPos);
        }

        private void SpawnPixelPiece(Vector3 worldPos, Color color, Vector2 hitDirection)
        {
            GameObject piece = Instantiate(pixelPiecePb, worldPos, Quaternion.identity);

            Sprite sprite = targetSpriteRenderer.sprite;

            float pixelWorldSize = 1f / sprite.pixelsPerUnit;
            float pieceWorldSize = sampleStep * pixelWorldSize;

            if (piece.TryGetComponent(out PixelPiece pixelPiece))
            {
                pixelPiece.Setup
                (
                    color,
                    pieceWorldSize,
                    targetSpriteRenderer.sortingLayerID,
                    targetSpriteRenderer.sortingOrder + 1
                );
            }

            if (piece.TryGetComponent(out Rigidbody2D rb))
            {
                Vector2 randomDir = Random.insideUnitCircle.normalized;
                Vector2 finalDir = hitDirection + randomDir * randomForce + Vector2.up * upwardForce;

                rb.linearVelocity = finalDir.normalized * Random.Range(explosionForce * 0.5f, explosionForce);
                rb.angularVelocity = Random.Range(-angularForce, angularForce);
            }

            Destroy(piece, lifeTime);
        }
    }
}
