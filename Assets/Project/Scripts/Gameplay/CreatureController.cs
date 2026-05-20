using System.Collections;
using UnityEngine;

namespace TRPG.Runtime
{
    [DisallowMultipleComponent]
    public abstract class CreatureController : MonoBehaviour, ISelectable
    {
        [SerializeField, ReadOnly] private SpriteRenderer spriteRenderer = null;

        [SerializeField, ReadOnly] private CreatureModel model = null;


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
        public void Hit(float damage)
        {
            model.SetHp(model.Hp - damage);
        }
    }
}
