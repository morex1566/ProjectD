using System;
using UnityEngine;

namespace TRPG.Runtime
{
    [Flags]
    public enum WorldTileFlag
    {
        None = 0,
        Gate = 1 << 0,
        Road = 1 << 1,
        Spawnable = 1 << 2,
        Building = 1 << 3,
        Enviroment = 1 << 4,
    }

    [Serializable]
    public struct WorldTile
    {
        public Vector3Int CellPosition;
        public WorldTileFlag Flag;
    }
}
