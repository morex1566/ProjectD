using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 0, 0을 최좌하단으로 하는 월드의 청크를 관리합니다.
    /// </summary>
    public class WorldMap
    {
        private readonly Dictionary<Vector2Int, WorldChunk> chunks = new();

        public IReadOnlyDictionary<Vector2Int, WorldChunk> Chunks => chunks;
    }
}
