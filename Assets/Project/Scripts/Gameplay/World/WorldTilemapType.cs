using UnityEngine;

namespace TRPG.Runtime
{
    public enum WorldTilemapType
    {
        None = 0,
        WorldTilemapDefault = 1 << 0,
        WorldTilemapBackground = 1 << 1,
        WorldTilemapGround = 1 << 2,
        WorldTilemapUI = 1 << 3,
    }
    
    public static class WorldTilemapTypeEx
    {
        ///<summary>
        /// WorldTilemapType을 Unity LayerMask로 변환합니다.
        ///</summary>
        public static LayerMask ToLayerMask(this WorldTilemapType tilemapType)
        {
            return ToLayerMaskValue(tilemapType);
        }

        ///<summary>
        /// WorldTilemapType을 Unity LayerMask int 값으로 변환합니다.
        ///</summary>
        public static int ToLayerMaskValue(this WorldTilemapType tilemapType)
        {
            // None은 실제 레이어가 아니므로 빈 마스크를 반환합니다.
            if (tilemapType == WorldTilemapType.None)
            {
                return 0;
            }

            // enum 이름과 같은 Unity Layer를 찾습니다.
            int layer = LayerMask.NameToLayer(tilemapType.ToString());
            if (layer < 0)
            {
                Debug.LogWarning($"Unity Layer가 존재하지 않습니다: {tilemapType}");
                return 0;
            }

            // Layer 번호를 LayerMask 비트로 변환합니다.
            return 1 << layer;
        }
    }
}
