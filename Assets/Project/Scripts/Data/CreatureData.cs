using System;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 플레이어와 몬스터가 공유하는 기본 크리처 스탯 데이터입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_Creature", menuName = "Scriptable Objects/Data/Creature")]
    public class CreatureData : ScriptableObject
    {
        public string Id;

        public string DisplayName;

        public string DefaultSkillId;

        public float Hp;

        public float Damage;

        public float Armor;

        public string Description;

        public SkillData SkillData;
    }
}
