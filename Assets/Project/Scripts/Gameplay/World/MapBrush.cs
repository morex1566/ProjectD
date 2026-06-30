using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TRPG.Runtime
{
    /// <summary>
    /// 하나의 논리 타일 타입에 사용할 후보 타일과 출현 가중치를 보관합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_MapBrush", menuName = "Scriptable Objects/Map/Brush")]
    public class MapBrush : ScriptableObject
    {
        /// <summary>
        /// 무작위 선택에 사용되는 단일 타일 후보입니다.
        /// </summary>
        [Serializable]
        public class WeightedTile
        {
            [SerializeField] public TileBase Tile;

            [SerializeField, Min(1)] public int Weight = 1;
        }

        [SerializeField] private MapTileType tileType = MapTileType.Ground;

        [SerializeField] private List<WeightedTile> tiles = new();

        public MapTileType TileType => tileType;

        /// <summary>
        /// 브러시 하나에는 단일 타일 타입만 지정되도록 보정합니다.
        /// </summary>
        private void OnValidate()
        {
            int value = (int)tileType;
            if (value > 0 && (value & (value - 1)) == 0) return;

            tileType = MapTileType.Ground;
        }

        /// <summary>
        /// 가중치에 따라 타일 하나를 선택합니다.
        /// </summary>
        public bool TryGetRandomTile(out TileBase tile)
        {
            tile = null;
            int totalWeight = 0;

            for (int i = 0; i < tiles.Count; i++)
            {
                WeightedTile candidate = tiles[i];
                if (candidate == null || candidate.Tile == null || candidate.Weight <= 0) continue;

                totalWeight += candidate.Weight;
            }

            if (totalWeight <= 0) return false;

            int randomWeight = UnityEngine.Random.Range(0, totalWeight);

            for (int i = 0; i < tiles.Count; i++)
            {
                WeightedTile candidate = tiles[i];
                if (candidate == null || candidate.Tile == null || candidate.Weight <= 0) continue;

                if (randomWeight < candidate.Weight)
                {
                    tile = candidate.Tile;
                    return true;
                }

                randomWeight -= candidate.Weight;
            }

            return false;
        }

        /// <summary>
        /// 지정한 타일이 후보 목록에 포함되어 있는지 확인합니다.
        /// </summary>
        public bool Contains(TileBase tile)
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                WeightedTile candidate = tiles[i];
                if (candidate != null && candidate.Tile == tile)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
