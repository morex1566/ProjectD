using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 몬스터 전용 데이터 접근을 제공하는 크리처 모델입니다.
    /// </summary>
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
