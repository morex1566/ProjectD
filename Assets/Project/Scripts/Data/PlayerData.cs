using UnityEngine;

namespace TRPG.Runtime
{
    [CreateAssetMenu(fileName = "SO_Player_00", menuName = "Scriptable Objects/Creature/Player")]
    public class PlayerData : CreatureData
    {
        public float MoveDelay = 0.3f;

        public float AttackDelay = 0.2f;
    }
}
