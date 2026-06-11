using System;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// CreatureData를 복사해서 생성되는 전투 중 상태값입니다.
    /// </summary>
    public class CreatureStatus
    {
        public float CurrentHp;

        public float Atk;

        public float DetectRange;

        public bool IsDead;

        public float AttackRange;

        public float AttackSpeed;

        public float MoveSpeed;

        public CreatureStatus(CreatureData data)
        {
            // CreatureData의 전투 수치를 런타임 상태로 복사합니다.
            CurrentHp = data.Hp;
            Atk = data.Atk;
            DetectRange = data.DetectRange;
            AttackRange = data.AttackRange;
            AttackSpeed = data.AttackSpeed;
            MoveSpeed = data.MoveSpeed;
            IsDead = CurrentHp <= 0f;
        }
    }
}
