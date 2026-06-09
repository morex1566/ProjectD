using UnityEngine;

namespace TRPG.Runtime
{
    public class CreatureController : MonoBehaviour, ISelectable
    {
        [SerializeField] private Sprite creatureSprite;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Collider2D selectionCollider;
        [SerializeField] private Color selectedColor = Color.cyan;
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float stopDistance = 0.01f;

        private Vector3 targetPosition;
        private bool hasTarget;
        private Color defaultColor = Color.white;

        public bool CanSelect { get; set; } = true;

        public bool IsSelected { get; set; }

        public Bounds SelectionBounds
        {
            get
            {
                if (spriteRenderer != null)
                {
                    return spriteRenderer.bounds;
                }

                if (selectionCollider != null)
                {
                    return selectionCollider.bounds;
                }

                return new Bounds(transform.position, Vector3.zero);
            }
        }

        private void Awake()
        {
            targetPosition = transform.position;
            spriteRenderer ??= GetComponentInChildren<SpriteRenderer>();
            selectionCollider ??= GetComponentInChildren<Collider2D>();

            if (spriteRenderer != null)
            {
                defaultColor = spriteRenderer.color;
                if (creatureSprite != null)
                {
                    spriteRenderer.sprite = creatureSprite;
                }
            }
        }

        private void Update()
        {
            MoveToTarget();
        }

        public bool Contains(Vector3 worldPosition)
        {
            if (selectionCollider != null)
            {
                return selectionCollider.OverlapPoint(worldPosition);
            }

            return SelectionBounds.Contains(worldPosition);
        }

        public void SetSelected(bool isSelected)
        {
            IsSelected = isSelected;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = isSelected ? selectedColor : defaultColor;
            }
        }

        /// <summary>
        /// 외부 입력 처리자가 전달한 월드 좌표를 이동 목표로 설정합니다.
        /// </summary>
        public void MoveTo(Vector3 worldPosition)
        {
            targetPosition = new Vector3(worldPosition.x, worldPosition.y, transform.position.z);
            hasTarget = true;
        }

        /// <summary>
        /// 현재 위치에서 목표 지점까지 일정 속도로 이동합니다.
        /// </summary>
        private void MoveToTarget()
        {
            if (!hasTarget) return;

            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPosition) <= stopDistance)
            {
                transform.position = targetPosition;
                hasTarget = false;
            }
        }
    }
}
