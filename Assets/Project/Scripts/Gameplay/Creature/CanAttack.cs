using MBT;
using UnityEngine;

namespace TRPG.Runtime
{
    [AddComponentMenu("")]
    [MBTNode(name = "Creature/Condition - CanAttack")]
    public class CanAttack : Condition
    {
        [SerializeField, ReadOnly] private CreatureController controller = null;

        private void OnValidate()
        {
            CacheComponents();
        }

        private void Awake()
        {
            CacheComponents();
        }

        public override bool Check()
        {
            return controller.IsDead() == false;
        }

        private void CacheComponents()
        {
            controller = GetComponentInParent<CreatureController>();
        }
    }
}
