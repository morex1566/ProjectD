using UnityEngine;

namespace TRPG.Runtime
{
    public class PlayerModel : CreatureModel
    {
        public PlayerData Data => data as PlayerData;
    }
}
