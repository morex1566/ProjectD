using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 몬스터 크리처의 런타임 입력과 행동을 담당하는 컨트롤러입니다.
    /// </summary>
    public class MonsterController : CreatureController
    {
        public new MonsterModel Model => base.Model as MonsterModel;

    }
}
