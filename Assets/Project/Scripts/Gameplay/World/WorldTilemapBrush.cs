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
        /// 무작위 선택에 사용되는 단일 타일 후보입니다.
        /// </summary>
        [Serializable]
        public class WeightedTile
        {
            [SerializeField] public TileBase Tile;

            [SerializeField, Min(1)] public int Weight = 1;

            [SerializeField] public float Gravity;
        }

        [SerializeField] private WorldTileType tileType = WorldTileType.Ground;

        [SerializeField] private List<WeightedTile> tiles = new();

        public WorldTileType TileType => tileType;



        /// <summary>
        /// 브러시 하나에는 단일 타일 타입만 지정되도록 보정합니다.
        /// </summary>
        private void OnValidate()
        {
            int value = (int)tileType;
            if (value > 0 && (value & (value - 1)) == 0) return;

            tileType = WorldTileType.Ground;
        }

        /// <summary>
        /// 가중치에 따라 타일 하나를 선택합니다.
        /// </summary>
        public bool TryGetRandomTile(out TileBase tile)
        {
            return TryGetRandomTile(out tile, out _);
        }

        /// <summary>
        /// 가중치에 따라 타일 하나와 해당 타일의 중력 값을 선택합니다.
        /// </summary>
        public bool TryGetRandomTile(out TileBase tile, out float gravity)
        {
            tile = null;
            gravity = 0f;
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
                    gravity = candidate.Gravity;
                    return true;
                }

                randomWeight -= candidate.Weight;
            }

            return false;
        }

        /// <summary>
        /// 저장된 맵을 재현할 때 사용할 기본 타일을 반환합니다.
        /// </summary>
        public bool TryGetDefaultTile(out TileBase tile)
        {
            tile = null;

            for (int i = 0; i < tiles.Count; i++)
            {
                WeightedTile candidate = tiles[i];
                if (candidate == null || candidate.Tile == null) continue;

                tile = candidate.Tile;
                return true;
            }

            return false;
        }
    }
}
