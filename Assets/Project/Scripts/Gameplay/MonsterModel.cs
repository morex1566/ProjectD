using UnityEngine;

namespace TRPG.Runtime
{
    [RequireComponent(typeof(MonsterController))]
    public class MonsterModel : CreatureModel
    {
        public CreatureData Data => data as MonsterData;

        public override void Init(Vector3Int cellPos, CreatureData data)
        {
            // 몬스터의 경우 데이터를 따로 로드해야함
            base.Init(cellPos, data);
        }
    }
}
