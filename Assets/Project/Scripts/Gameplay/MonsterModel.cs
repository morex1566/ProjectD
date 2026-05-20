using UnityEngine;

namespace TRPG.Runtime
{
    public class MonsterModel : CreatureModel
    {
        public MonsterData Data => data as MonsterData;
    }
}
