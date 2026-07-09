using MBT;
using UnityEngine;

namespace TRPG.Runtime
{
    [AddComponentMenu("")]
    [MBTNode(name = "Creature/Leaf - DoMining")]
    public class DoMining : Leaf
    {
        public override NodeResult Execute()
        {
            return NodeResult.running;
        }
    }
}
