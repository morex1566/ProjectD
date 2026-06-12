using System;
using UnityEngine;

namespace TRPG.Runtime
{
    public enum MapTileType
    {
        Sea,
        Land
    }

    /// <summary>
    /// 맵 정보를 저장
    /// </summary>
    [Serializable]
    public class Map
    {
        public float[,] heights;
    }
}
