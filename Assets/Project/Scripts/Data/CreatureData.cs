using System;
using UnityEngine;

namespace TRPG.Runtime
{
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

        public GameObject SpritePf;
        public GameObject CreaturePf;

        /// <summary>
        /// 셋업 데이터를 기준으로 독립적인 런타임 상태를 생성합니다.
        /// </summary>
        public CreatureStatus Create()
        {
            return new CreatureStatus(this);
        }
    }
}
