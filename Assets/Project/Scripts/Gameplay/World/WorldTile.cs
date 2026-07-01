using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TRPG.Runtime
{
    /// <summary>
    /// 맵의 단일 셀에 저장되는 데이터입니다.
    /// </summary>
    [Serializable]
    public struct WorldTile
    {
        [SerializeField, ReadOnly] public Vector3Int Pos;

        [SerializeField] public WorldTileType Type;

        [SerializeField] public float Gravity;

        [SerializeField] public TileBase TileBase;
    }
}
