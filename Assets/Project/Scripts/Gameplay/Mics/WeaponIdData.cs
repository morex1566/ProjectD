using System;
using UnityEngine;

namespace TRPG.Runtime
{
    [Serializable]
    [CreateAssetMenu(fileName = "SO_WeaponId_", menuName = "Scriptable Objects/Weapon/WeaponId")]
    public class WeaponIdData : ScriptableObject
    {
        [SerializeField, ReadOnly] public string Id;
    }
}
