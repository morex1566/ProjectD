using MBT;
using UnityEngine;

namespace TRPG.Runtime
{
    [AddComponentMenu("")]
    [MBTNode(name = "Creature/Leaf - DoAttack")]
    public class DoAttack : Leaf
    {
        public override NodeResult Execute()
        {
            return NodeResult.running;
        }
    }
}
