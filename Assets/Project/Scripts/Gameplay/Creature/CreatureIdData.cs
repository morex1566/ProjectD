using System;
using UnityEngine;

namespace TRPG.Runtime
{
    [Serializable]
    [CreateAssetMenu(fileName = "SO_CreatureId_", menuName = "Scriptable Objects/Creature/Creature Id")]
    public class CreatureIdData : ScriptableObject
    {
        [SerializeField, ReadOnly] public string Id;
    }
}
