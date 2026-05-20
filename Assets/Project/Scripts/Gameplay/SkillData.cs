using System;
using UnityEngine;

namespace TRPG.Runtime
{
    [Serializable]
    public abstract class SkillData : ScriptableObject
    {
        [Header("Setup")]
        [field: SerializeField] public int moveRange;
        [field: SerializeField] public int damage;
    }
}
