using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TRPG.Runtime
{
    /// <summary>
    /// 저장 가능한 맵 원본 데이터입니다.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "SO_Map", menuName = "Scriptable Objects/WorldGridController")]
    public class WorldGridData : ScriptableObject
    {
        [SerializeField] private SerializableDictionary<WorldTilemapType, WorldTilemapData> tilemapDatas = new();
    }
}
