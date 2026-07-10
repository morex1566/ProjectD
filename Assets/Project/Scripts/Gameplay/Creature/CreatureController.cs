using System;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// Creature의 런타임 상태, 선택 상태, Job 큐를 관리하는 월드 컴포넌트입니다.
    /// </summary>
    public partial class CreatureController : MonoBehaviour, ISelectable
    {
        [SerializeField, ReadOnly] private SpriteRenderer spriter = null;

        [SerializeField] private CreatureContext context = null;

        private CreatureJobQueue jobQueue = null;


        public bool CanSelect { get; set; } = true;

        public bool IsSelected { get; set; } = false;

        public Bounds SelectionBounds => spriter.bounds;

        public GameObject SelectedInst => gameObject;

        public int InstanceId => gameObject.GetInstanceID();

        public SpriteRenderer Spriter => spriter;

        public CreatureJobQueue JobQueue => jobQueue;

        public CreatureContext Context => context;

        public WeaponContext EquippedWeapon => context.EquippedWeapon;

        /// <summary>
        /// in - creaturecontroller is Victim
        /// </summary>
        public Action<CreatureController> OnHit;


        private void OnValidate()
        {
            CacheComponents();
        }

        private void Awake()
        {
            CacheComponents();
            Init();
        }

        /// <summary>
        /// 인스턴스 아이디 표기
        /// </summary>
        private void OnDrawGizmos()
        {
            // 선택 범위 박스
            if (spriter != null)
            {
                Gizmos.color = IsSelected ? Color.green : Color.yellow;
                Gizmos.DrawWireCube(SelectionBounds.center, SelectionBounds.size);
            }

            if (Application.isPlaying == false)
            {
                return;
            }
        }


        /// <summary>
        /// 프리팹에 저장된 런타임 상태를 사용할 수 있도록 보정합니다.
        /// </summary>
        public void Init()
        {
            jobQueue = new CreatureJobQueue(this);
        }

        /// <summary>
        /// 월드 좌표가 선택 판정 Bounds 안에 있는지 확인합니다.
        /// </summary>
        public bool Contains(Vector3 worldPosition)
        {
            return SelectionBounds.Contains(worldPosition);
        }

        /// <summary>
        /// 선택 상태를 갱신하고 선택 표시 오브젝트를 토글합니다.
        /// </summary>
        public void SetSelected(bool isSelected)
        {
            IsSelected = isSelected;
        }

        public bool IsDead()
        {
            return context.Hp <= 0f;
        }

        private void CacheComponents()
        {
            spriter = GetComponentInChildren<SpriteRenderer>();
        }

        /// <summary>
        /// Weapon id를 조회해서 현재 Creature의 무기로 장착합니다.
        /// </summary>
        public bool EquipWeapon(string weaponId)
        {
            if (WorldManager.TryGetWeaponData(weaponId, out WeaponData weaponData) == false)
            {
                Debug.LogWarning($"EquipWeapon failed. WeaponData not found. Id: {weaponId}", this);
                return false;
            }

            return EquipWeapon(weaponData);
        }

        /// <summary>
        /// WeaponData를 런타임 장착 상태로 복사하고 전투 스탯을 다시 계산합니다.
        /// </summary>
        public bool EquipWeapon(WeaponData weaponData)
        {
            if (weaponData == null)
            {
                Debug.LogWarning("EquipWeapon failed. WeaponData is null.", this);
                return false;
            }

            context.EquippedWeapon.Load(weaponData);

            return true;
        }

        /// <summary>
        /// 월드에 배치된 WeaponController의 현재 상태를 복사해서 장착합니다.
        /// </summary>
        public bool EquipWeapon(WeaponController weaponController)
        {
            if (weaponController == null || weaponController.Context == null)
            {
                Debug.LogWarning("EquipWeapon failed. WeaponController is null or not initialized.", this);
                return false;
            }

            context.EquippedWeapon.Load(weaponController.Context);

            return true;
        }

        /// <summary>
        /// 현재 장착 중인 무기를 해제하고 Creature 기본 전투 스탯으로 되돌립니다.
        /// </summary>
        public void UnequipWeapon()
        {
            context.EquippedWeapon.Clear();
        }

        public void TakeDamage(int damage)
        {
            context.Hp -= damage;

            OnHit?.Invoke(this);
        }
    }
}
