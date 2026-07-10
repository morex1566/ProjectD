using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 월드에 배치되는 Weapon 오브젝트의 표시와 런타임 상태를 관리합니다.
    /// </summary>
    public class WeaponController : MonoBehaviour
    {
        [SerializeField, ReadOnly] private SpriteRenderer spriter = null;

        [SerializeField] private WeaponContext context = null;

        public SpriteRenderer Spriter => spriter;

        public WeaponContext Context => context;

        private void OnValidate()
        {
            CacheComponents();
        }

        private void Awake()
        {
            CacheComponents();
            Init();
        }

        public void Init()
        {
            if (context == null)
            {
                context = new WeaponContext();
            }
        }

        private void CacheComponents()
        {
            if (spriter == null)
            {
                spriter = GetComponentInChildren<SpriteRenderer>(true);
            }
        }
    }
}
