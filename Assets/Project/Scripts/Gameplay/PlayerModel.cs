using UnityEngine;

namespace TRPG.Runtime
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerModel : CreatureModel
    {
        public PlayerData Data => data as PlayerData;

        public override void Init(Vector3Int cellPos, CreatureData data = null)
        {
            // 플레이어의 경우 이미 데이터가 프리팹에 들어있음
            base.Init(cellPos, this.data);
        }
    }
}
