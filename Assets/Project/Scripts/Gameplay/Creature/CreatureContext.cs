using System;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// CreatureContext AI의 이동/행동 타입입니다.
    /// </summary>
    public enum CreatureAIType
    {
        None,
        Ground,
        Air
    }

    /// <summary>
    /// CreatureData를 복사해서 생성되는 전투 중 상태값입니다.
    /// </summary>
    [Serializable]
    public class CreatureContext
    {
        public float CurrentHp;

        public float Atk;

        public float DetectRange;

        public float AttackRange;

        public float AttackSpeed;

        public float MoveSpeed;

        public CreatureAIType AIType = CreatureAIType.None;

        public CreatureStateType State = CreatureStateType.None;
    }
}
