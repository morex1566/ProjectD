using UnityEngine;

namespace TRPG.Runtime
{
    public class PixelBreaker : MonoBehaviour
    {
        [Header(nameof(PixelBreaker))]
        [SerializeField] private SpriteRenderer targetSpriteRenderer;
        [SerializeField] private GameObject pixelPiecePb;

        [Header("Pixel Piece")]
        [SerializeField] private int sampleStep = 4;
        [SerializeField] private int borderPixelWidth = 1;
        [SerializeField] private Color borderColor = new Color(0.08f, 0.08f, 0.08f, 1f);

        [Header("Explosion")]
        [SerializeField] private float explosionForce = 4f;
        [SerializeField] private float randomForce = 0.5f;
        [SerializeField] private float upwardForce = 1.2f;
        [SerializeField] private float angularForce = 360f;
        [SerializeField] private float lifeTime = 0.6f;

        public void Break(Vector2 hitDirection)
        {
            if (targetSpriteRenderer == null) return;
            if (targetSpriteRenderer.sprite == null) return;
            if (pixelPiecePb == null) return;

            Sprite sprite = targetSpriteRenderer.sprite;
            Texture2D texture = sprite.texture;
            Rect textureRect = sprite.textureRect;

            hitDirection = hitDirection.normalized;

            for (int y = 0; y < textureRect.height; y += sampleStep)
            {
                for (int x = 0; x < textureRect.width; x += sampleStep)
                {
                    int textureX = (int)textureRect.x + x;
                    int textureY = (int)textureRect.y + y;

                    Color pixelColor = texture.GetPixel(textureX, textureY);

                    if (pixelColor.a <= 0.1f) continue;

                    Vector3 worldPos = GetPixelWorldPos(sprite, x, y);

                    SpawnPixelPiece(worldPos, pixelColor, hitDirection);
                }
            }

            targetSpriteRenderer.enabled = false;
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
            float borderWorldSize = borderPixelWidth * pixelWorldSize;

            if (piece.TryGetComponent(out PixelPiece pixelPiece))
            {
                pixelPiece.Setup
                (
                    color,
                    borderColor,
                    pieceWorldSize,
                    borderWorldSize,
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