using UnityEngine;

namespace TRPG.Runtime
{
    public class PixelPiece : MonoBehaviour
    {
        [Header(nameof(PixelPiece))]
        [SerializeField] private SpriteRenderer fillRenderer;

        public void Setup(Color fillColor, float pieceWorldSize, int sortingLayerID, int sortingOrder)
        {
            if (fillRenderer == null) return;

            fillRenderer.color = fillColor;
            fillRenderer.sortingLayerID = sortingLayerID;
            fillRenderer.sortingOrder = sortingOrder;

            // Border가 없으므로 fill이 piece 전체 크기를 그대로 차지합니다.
            float fillWorldSize = Mathf.Max(0f, pieceWorldSize);
            SetSpriteWorldSize(fillRenderer, fillWorldSize);
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
