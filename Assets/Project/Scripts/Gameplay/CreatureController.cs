using System.Collections;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 캐릭터 조작 클래스
    /// </summary>
    [DisallowMultipleComponent]
    public abstract partial class CreatureController : MonoBehaviour, ISelectable
    {
        [Header("CreatureController")]

        [SerializeField, ReadOnly] private SpriteRenderer spriteRenderer = null;

        [SerializeField, ReadOnly] private CreatureModel model = null;

        [SerializeField] private Color hitFlashColor = Color.red;

        [SerializeField] private float hitFlashDuration = 0.08f;

        /// <summary>
        /// 맞았을 때 효과
        /// </summary>
        private Coroutine hitFlashCoroutine;


        public bool CanSelect { get; set; } = false;

        public bool IsSelected { get; set; } = false;

        public CreatureModel Model => model;


        private void OnValidate()
        {
            Init();
        }

        protected virtual void Awake()
        {
            Init();
        }

        private void Init()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            model = GetComponent<CreatureModel>();
        }

        private void OnEnable()
        {

        }

        private void OnDisable()
        {

        }

        /// <summary>
        /// 좌표가 이 크리처의 렌더러 영역 안에 있는지 검사합니다.
        /// </summary>
        public bool Contains(Vector3 position)
        {
            position.z = spriteRenderer.bounds.center.z;

            return spriteRenderer.bounds.Contains(position);
        }

        /// <summary>
        /// 현재 선택 상태를 저장합니다.
        /// </summary>
        public void SetSelected(bool isSelected)
        {
            IsSelected = isSelected;
        }

        /// <summary>
        /// 피해량만큼 HP를 감소시킵니다.
        /// </summary>
        public void Hit(CreatureController attacker, float damage)
        {
            // 로직
            float calculatedDamage = Mathf.Clamp(damage - model.Armor, 0f, 99999);
            model.SetHp(model.Hp - calculatedDamage);

            // 뷰
            // 맞는 효과
            // 플레이어와 대상의 사이에서 Damage UI콜
            PlayHitFlash();
            Vector3 center = (attacker.transform.position + transform.position) * 0.5f;
            UIManager.GetInstance().ShowDamage(center, calculatedDamage);

            if (CheckIsDead()) WorldManager.GetInstance().Despawn(GetInstanceID());
        }

        private bool CheckIsDead()
        {
            return model.Hp <= 0;
        }
    }

    // 뷰관련 로직
    public abstract partial class CreatureController : MonoBehaviour, ISelectable
    {
        private void PlayHitFlash()
        {
            if (hitFlashCoroutine != null) StopCoroutine(hitFlashCoroutine);

            hitFlashCoroutine = StartCoroutine(HitFlashCo());
        }

        private IEnumerator HitFlashCo()
        {
            Color originColor = spriteRenderer.color;

            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(hitFlashDuration);

            spriteRenderer.color = originColor;
            hitFlashCoroutine = null;
        }
    }
}
