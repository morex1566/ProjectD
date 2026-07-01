using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TRPG.Runtime
{
    /// <summary>
    /// 하나의 논리 타일 타입에 사용할 후보 타일과 출현 가중치를 보관합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_TilemapBrush", menuName = "Scriptable Objects/World/TilemapBrush")]
    public class WorldTilemapBrush : ScriptableObject
    {
        /// <summary>
        /// Key : 가중치, Value : 후보 타일
        /// </summary>
        [SerializeField] private SerializableDictionary<int, WorldTile> tiles = new();

        /// <summary>
        /// 가중치에 따라 후보 타일 하나를 선택합니다.
        /// </summary>
        public bool TryGetRandomTile(out WorldTile tile)
        {
            tile = default;
            Dictionary<int, WorldTile> candidates = tiles.ToDictionary();
            int totalWeight = 0;

            foreach (KeyValuePair<int, WorldTile> pair in candidates)
            {
                if (pair.Key <= 0 || pair.Value.TileBase == null)
                {
                    continue;
                }

                totalWeight += pair.Key;
            }

            if (totalWeight <= 0)
            {
                return false;
            }

            int randomWeight = UnityEngine.Random.Range(0, totalWeight);

            foreach (KeyValuePair<int, WorldTile> pair in candidates)
            {
                if (pair.Key <= 0 || pair.Value.TileBase == null)
                {
                    continue;
                }

                if (randomWeight < pair.Key)
                {
                    tile = pair.Value;
                    return true;
                }

                randomWeight -= pair.Key;
            }

            return false;
        }
    }
}
