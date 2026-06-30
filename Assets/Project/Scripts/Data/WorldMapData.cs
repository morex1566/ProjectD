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
    [CreateAssetMenu(fileName = "SO_Map", menuName = "Scriptable Objects/WorldMapController")]
    public class WorldMapData : ScriptableObject
    {
        [SerializeField] private Vector3Int pivot = Vector3Int.zero;

        [SerializeField] private Vector3Int startSpawnPoint = Vector3Int.zero;

        [SerializeField] private SerializableDictionary<WorldTilemapType, WorldTilemapData> tilemapDatas = new();

        public Vector3Int Pivot => pivot;

        public Vector3Int StartSpawnPoint => startSpawnPoint;
    }
}
