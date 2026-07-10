using System;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 엑셀에서 로드되는 Weapon의 정적 설정 데이터입니다.
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
        public string PrefabPath;
        public Sprite Sprite;
        public GameObject Prefab;
    }
}
