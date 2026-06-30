using System;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 맵의 단일 셀에 저장되는 데이터입니다.
    /// </summary>
    [Serializable]
    public struct WorldTile
    {
        public WorldTileType Type;
        public Vector2Int Pos;
        public float Gravity;
    }
}
