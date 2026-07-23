using System;

namespace TRPG.Runtime
{
    /// <summary>
    /// WeaponSheet 엑셀에서 로드되는 무기 데이터입니다.
    /// </summary>
    [Serializable]
    public class WeaponData
    {
        public string Id;
        public string Name;
        public string Description;
        public float Damage;
        public float AttackRange;
        public float AttackSpeed;
        public float Weight;
    }
}
