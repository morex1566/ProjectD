using System;
using UnityEngine;

namespace TRPG.Runtime
{
    [Serializable]
    public class CreatureData : ScriptableObject
    {
        [ReadOnly] public string Id;

        [ReadOnly] public string Description;

        [ReadOnly] public string DisplayName;

        [ReadOnly] public string Type;

        [ReadOnly] public string DefaultSkillId;

        [ReadOnly] public int Hp;

        [ReadOnly] public int Damage;

        [ReadOnly] public int Armor;

        public MoveRangeData MoveRangeData;
    }
}
