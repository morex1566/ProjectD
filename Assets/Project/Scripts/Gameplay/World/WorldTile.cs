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
        [SerializeField, ReadOnly] public GameObject Owner;

        [SerializeField, ReadOnly] public WorldTileType Type;

        [SerializeField, ReadOnly] public Vector2Int Pos;

        [SerializeField, ReadOnly] public float Gravity;

        [SerializeField, ReadOnly] public TileBase TileBase;
    }
}
