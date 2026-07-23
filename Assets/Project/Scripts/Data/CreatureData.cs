using System;

namespace TRPG.Runtime
{
    /// <summary>
    /// CreatureSheet 엑셀에서 로드되는 생물 데이터입니다.
    /// </summary>
    [Serializable]
    public class CreatureData
    {
        public string Id;
        public string Name;
        public string Description;
        public string Faction;
        public float Hp;
        public float Damage;
        public float DetectRange;
        public float AttackRange;
        public float AttackSpeed;
        public float MoveSpeed;
        public string AIType;
    }
}
