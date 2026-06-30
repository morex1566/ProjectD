using System;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 맵 타일 타입입니다.
    /// </summary>
    [Flags]
    public enum WorldTileType
    {
        None = 0,
        Ground = 1 << 0,
        GroundSurface = 1 << 1,
        Air = 1 << 2,
        Background = 1 << 3,
    }
}
