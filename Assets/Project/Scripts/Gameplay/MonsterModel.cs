using UnityEngine;

namespace TRPG.Runtime
{
    [RequireComponent(typeof(MonsterController))]
    public class MonsterModel : CreatureModel
    {
        public MonsterData Data => data as MonsterData;

        public override void Init(CreatureData data, Vector3Int cellPos)
        {
            base.Init(data, cellPos);

            base.data = data as MonsterData;
        }
    }
}
