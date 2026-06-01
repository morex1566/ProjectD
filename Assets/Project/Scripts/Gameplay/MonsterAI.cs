using UnityEngine;
using System.Collections.Generic;

namespace TRPG.Runtime
{
    public readonly struct AIMove
    {
        public readonly CreatureController Actor;
        public readonly Vector3Int From;
        public readonly Vector3Int To;
        public readonly CreatureController Target;

        public bool IsAttack => Target != null;

        public AIMove(CreatureController actor, Vector3Int from, Vector3Int to, CreatureController target)
        {
            Actor = actor;
            From = from;
            To = to;
            Target = target;
        }
    }


    public class MonsterAI
    {
        
    }
}
