using UnityEngine;

namespace TRPG.Runtime
{
    [CreateAssetMenu(fileName = "SO_Player", menuName = "Scriptable Objects/Creature/Player")]
    public class PlayerData : CreatureData
    {
        public float Damage;
    }
}
