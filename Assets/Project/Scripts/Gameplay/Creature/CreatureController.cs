using System;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// Creature의 런타임 상태를 관리하는 월드 컴포넌트입니다.
    /// </summary>
    public class CreatureController : MonoBehaviour
    {
        [SerializeField, ReadOnly] private SpriteRenderer spriter = null;

        [SerializeField] private CreatureContext context = null;

        [SerializeField] private GameObject weaponInstance = null;

        public int InstanceId => gameObject.GetInstanceID();

        public SpriteRenderer Spriter => spriter;

        public CreatureContext Context => context;

        public GameObject WeaponInstance => weaponInstance;

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
        }

        public bool IsDead()
        {
            return context.Hp <= 0f;
        }

        private void CacheComponents()
        {
            spriter = GetComponentInChildren<SpriteRenderer>();
        }

        public void TakeDamage(int damage)
        {
            context.Hp -= damage;

            OnHit?.Invoke(this);
        }
    }
}
