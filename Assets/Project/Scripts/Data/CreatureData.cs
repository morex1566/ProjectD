using System;
using UnityEngine;

namespace TRPG.Runtime
{
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
