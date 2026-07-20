using UnityEngine;
using MBT;
using System.Collections.Generic;

namespace TRPG.Runtime
{
    [AddComponentMenu("")]
    [MBTNode(name = "Creature/Leaf - DoMove")]
    public class DoMove : Leaf
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

        public override NodeResult Execute()
        {
            if (controller == null || controller.IsDead() == true || controller.Context.MoveSpeed <= 0f)
            {
                return NodeResult.failure;
            }

            if (controller.JobQueue.TryPeek(out CreatureMoveJob job) == false)
            {
                return NodeResult.failure;
            }

            return NodeResult.running;
        }

        private void CacheComponents()
        {
            controller = GetComponentInParent<CreatureController>();
        }
    }
}
