using System;
using UnityEngine;

namespace TRPG.Runtime
{
    public enum WorldTileType
    {
        None,
        Gate,
        Road,
        Castle,
        Forest,
        Farm
    }

    [Flags]
    public enum WorldTileFlag
    {
        None = 0,
        Gate = 1 << 0,
        Road = 1 << 1,
        Spawnable = 1 << 2,
        Building = 1 << 3,
        Environment = 1 << 4,
    }

    [Serializable]
    public class WorldTile
    {
        public Vector3Int CellPosition;
        public WorldTileType Type;
        public WorldTileFlag Flag;
    }
}
