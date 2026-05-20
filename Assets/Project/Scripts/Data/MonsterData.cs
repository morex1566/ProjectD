using System;
using UnityEngine;

namespace TRPG.Runtime
{
    [CreateAssetMenu(fileName = "SO_Monster", menuName = "Scriptable Objects/Creature/Monster")]
    public class MonsterData : CreatureData
    {
        public string Id;

        public string DisplayName;

        public string PrefabAddress;

        public string DefaultSkillId;

        public float Hp;

        public float Damage;

        public float Armor;

        public string Description;

    }
}
