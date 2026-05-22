using UnityEngine;

namespace TRPG.Runtime
{
    public class MonsterController : CreatureController
    {
        public new MonsterModel Model => base.Model as MonsterModel;

    }
}
