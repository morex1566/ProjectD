using System;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 엑셀에서 로드되는 Creature의 정적 설정 데이터입니다.
    /// </summary>
    [Serializable]
    public class CreatureData
    {
        public string DataId;
        public string NameKey;
        public string DescKey;
        public string Faction;
        public float Hp;
        public float Damage;
        public float DetectRange;
        public float AttackRange;
        public float AttackSpeed;
        public float MoveSpeed;

        public CreatureAIType aiType;

        public GameObject SpritePf;
        public GameObject CreaturePf;
    }
}
