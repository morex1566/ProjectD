using UnityEngine;

namespace TRPG.Runtime
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerModel : CreatureModel
    {
        public PlayerData Data => data as PlayerData;

        public override void Init(CreatureData data, Vector3Int cellPos)
        {
            base.Init(data, cellPos);

            base.data = data as PlayerData;
        }
    }
}
