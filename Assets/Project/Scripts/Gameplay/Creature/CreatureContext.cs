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

    [Flags]
    public enum CreatureStateType
    {
        None = 0,
        Idle = 1 << 0,
        Move = 1 << 1,
        Mining = 1 << 2,
        Dead = 1 << 3
    }

    /// <summary>
    /// CreatureData를 복사해서 생성되는 전투 중 상태값입니다.
    /// </summary>
    [Serializable]
    public class CreatureContext
    {
        public float CurrentHp = 1;

        public float Atk = 1;

        public float DetectRange = 1;

        public float AttackRange = 1;

        public float AttackSpeed = 1;

        public float MoveSpeed = 1;

        public CreatureAIType AIType = CreatureAIType.None;

        public CreatureStateType State = CreatureStateType.None;
    }
}
