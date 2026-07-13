using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 월드 셀에 저장할 물질 종류입니다.
    /// </summary>
    public enum WorldMaterialType : byte
    {
        // not air, 그냥 물질이 없는 상태
        Empty = 0,
        Soil,
        Stone,
        Sand,
        Water,
    }
}
