using System;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// CreatureData를 복사해서 생성되는 전투 중 상태값입니다.
    /// </summary>
    public class CreatureContext
    {
        public float CurrentHp;

        public float Atk;

        public float DetectRange;

        public bool IsDead;

        public float AttackRange;

        public float AttackSpeed;

        public float MoveSpeed;

        /// <summary>
        /// 정적 CreatureData 값을 런타임에서 변경 가능한 상태값으로 복사합니다.
        /// </summary>
        public CreatureContext(CreatureData data)
        {
            // CreatureData의 전투 수치를 런타임 상태로 복사합니다.
            CurrentHp = data.Hp;
            Atk = data.Damage;
            DetectRange = data.DetectRange;
            AttackRange = data.AttackRange;
            AttackSpeed = data.AttackSpeed;
            MoveSpeed = data.MoveSpeed;
            IsDead = CurrentHp <= 0f;
        }
    }
}
