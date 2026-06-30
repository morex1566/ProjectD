using System;
using UnityEngine;

namespace TRPG.Runtime
{
    [Flags]
    public enum TilemapType
    {
        None = 0,
        Ground = 1 << 0,
        Background = 1 << 1,
        Selection = 1 << 2,
    }
}
