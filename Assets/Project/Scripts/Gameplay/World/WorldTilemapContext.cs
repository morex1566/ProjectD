using System;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 런타임에서 사용하는 단일 Tilemap 레이어 데이터입니다.
    /// </summary>
    [Serializable]
    public class WorldTilemapContext
    {
        [SerializeField] private WorldTilemapType tilemapType = WorldTilemapType.WorldTilemapDefault;


        public WorldTilemapType TilemapType
        {
            get => tilemapType;
            set => tilemapType = value;
        }
    }
}
