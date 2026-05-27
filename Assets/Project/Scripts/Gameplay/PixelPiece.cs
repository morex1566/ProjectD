using UnityEngine;

namespace TRPG.Runtime
{
    public class PixelPiece : MonoBehaviour
    {
        [Header(nameof(PixelPiece))]
        [SerializeField] private SpriteRenderer borderRenderer;
        [SerializeField] private SpriteRenderer fillRenderer;

        public void Setup(Color fillColor, Color borderColor, float pieceWorldSize, float borderWorldSize, int sortingLayerID, int sortingOrder)
        {
            if (borderRenderer != null)
            {
                borderRenderer.color = borderColor;
                borderRenderer.sortingLayerID = sortingLayerID;
                borderRenderer.sortingOrder = sortingOrder;
                SetSpriteWorldSize(borderRenderer, pieceWorldSize);
            }

            if (fillRenderer != null)
            {
                fillRenderer.color = fillColor;
                fillRenderer.sortingLayerID = sortingLayerID;
                fillRenderer.sortingOrder = sortingOrder + 1;

                float fillWorldSize = Mathf.Max(0f, pieceWorldSize - borderWorldSize * 2f);
                SetSpriteWorldSize(fillRenderer, fillWorldSize);
            }
        }

        private void SetSpriteWorldSize(SpriteRenderer spriteRenderer, float worldSize)
        {
            if (spriteRenderer.sprite == null) return;

            Vector2 spriteWorldSize = spriteRenderer.sprite.bounds.size;

            float scaleX = worldSize / spriteWorldSize.x;
            float scaleY = worldSize / spriteWorldSize.y;

            spriteRenderer.transform.localScale = new Vector3(scaleX, scaleY, 1f);
        }
    }
}