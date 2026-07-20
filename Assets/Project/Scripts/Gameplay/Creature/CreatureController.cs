using System;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// Creature의 런타임 상태를 관리하는 월드 컴포넌트입니다.
    /// </summary>
    public partial class CreatureController : MonoBehaviour
    {
        [SerializeField, ReadOnly] private SpriteRenderer spriter = null;

        [SerializeField] private BoxCollider2D hitBox = null;

        [SerializeField] private GameObject weaponInstance = null;

        [SerializeField] private CreatureContext context = null;

        private readonly CreatureJobQueue jobQueue = new();


        public int InstanceId => gameObject.GetInstanceID();

        public SpriteRenderer Spriter => spriter;

        public CreatureContext Context => context;

        public GameObject WeaponInstance => weaponInstance;

        public BoxCollider2D HitBox => hitBox;

        public CreatureJobQueue JobQueue => jobQueue;


        /// <summary>
        /// in - creaturecontroller is Victim
        /// </summary>
        public Action<CreatureController> OnHit;


        private void OnValidate()
        {
            CacheComponents();
        }

        private void OnEnable()
        {
            WorldManager.RegisterCreature(this);
        }

        private void OnDisable()
        {
            WorldManager.UnregisterCreature(this);
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
            hitBox ??= GetComponent<BoxCollider2D>();
        }

        /// <summary>
        /// CreatureJob을 기존 Job 뒤에 추가하거나 기존 Job을 교체합니다.
        /// </summary>
        public void EnqueueJob<T>(T job, PlayerCommandQueueMode mode) where T : CreatureJob
        {
            if (job == null)
            {
                return;
            }

            switch (mode)
            {
                case PlayerCommandQueueMode.Replace:
                    jobQueue.Clear();
                    jobQueue.Enqueue(job);
                    break;

                case PlayerCommandQueueMode.Append:
                    jobQueue.Enqueue(job);
                    break;
            }
        }

        public void TakeDamage(int damage)
        {
            context.Hp -= damage;

            OnHit?.Invoke(this);
        }
    }
}
