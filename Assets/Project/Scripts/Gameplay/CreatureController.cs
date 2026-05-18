using UnityEngine;

namespace TRPG.Runtime
{
    public class CreatureController : MonoBehaviour, ISelectable
    {
        [SerializeField] private bool canSelect = true;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color selectedColor = new Color(0.35f, 0.8f, 1f, 1f);
        [SerializeField] private float selectedScale = 1.1f;

        private Color defaultColor;
        private Vector3 defaultScale;

        public bool CanSelect => canSelect;
        public bool IsSelected { get; private set; }

        protected virtual void Awake()
        {
            if (!spriteRenderer)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            defaultColor = spriteRenderer.color;
            defaultScale = transform.localScale;
        }

        public bool ContainsScreenPosition(Vector2 screenPosition, Camera targetCamera)
        {
            Vector3 worldPosition = targetCamera.ScreenToWorldPoint(screenPosition);
            worldPosition.z = spriteRenderer.bounds.center.z;

            return spriteRenderer.bounds.Contains(worldPosition);
        }

        public virtual void SetSelected(bool isSelected)
        {
            IsSelected = isSelected;
            spriteRenderer.color = isSelected ? selectedColor : defaultColor;
            transform.localScale = isSelected ? defaultScale * selectedScale : defaultScale;
        }
    }
}
