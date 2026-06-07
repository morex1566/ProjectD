using UnityEngine;

namespace TRPG.Runtime
{
    public class CreatureAnimEventHandler : AnimEventHandler
    {
        [SerializeField] private CreatureController owner;

        private void Awake()
        {
            owner = GetComponentInParent<CreatureController>();
        }

        /// <summary>
        /// Despawn 애니메이션에서 트리거
        /// </summary>
        public void OnDespawn()
        {
            owner.Despawn();
        }
    }
}
