using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TRPG.Runtime
{
    public class WorldGridController : MonoBehaviour
    {
        [Header("Tilemap")]
        [SerializeField] private Tilemap tilemap;

        [Header("Tile Assets")]
        [SerializeField] private TileBase gateTile;
        [SerializeField] private TileBase roadTile;
        [SerializeField] private TileBase castleTile;
        [SerializeField] private TileBase forestTile;
        [SerializeField] private TileBase farmTile;


        private void OnValidate()
        {
            CacheComponents();
        }

        private void Awake()
        {
            CacheComponents();
        }

        /// <summary>
        /// 타일 배치
        /// </summary>
        public void SetTiles(IReadOnlyList<WorldTile> worldTiles)
        {
            for (int i = 0; i < worldTiles.Count; i++)
            {
                WorldTile worldTile = worldTiles[i];

                if (TryGetTileAsset(worldTile.Type, out TileBase tileAsset) == false)
                {
                    continue;
                }

                tilemap.SetTile(worldTile.CellPosition, tileAsset);
            }
        }

        /// <summary>
        /// 타일 배치
        /// </summary>
        public void SetTile(WorldTile worldTile)
        {
            if (TryGetTileAsset(worldTile.Type, out TileBase tileAsset) == false)
            {
                return;
            }

            tilemap.SetTile(worldTile.CellPosition, tileAsset);
        }

        /// <summary>
        /// 월드 위치를 Hex Tilemap의 셀 좌표로 변환합니다.
        /// </summary>
        public Vector3Int WorldToCell(Vector3 worldPosition)
        {
            return tilemap.WorldToCell(worldPosition);
        }

        /// <summary>
        /// 월드 타일 타입에 대응하는 시각적 타일 에셋을 반환합니다.
        /// </summary>
        private bool TryGetTileAsset(WorldTileType type, out TileBase tileAsset)
        {
            switch (type)
            {
                case WorldTileType.Gate:
                    tileAsset = gateTile;
                    break;

                case WorldTileType.Road:
                    tileAsset = roadTile;
                    break;

                case WorldTileType.Castle:
                    tileAsset = castleTile;
                    break;

                case WorldTileType.Forest:
                    tileAsset = forestTile;
                    break;

                case WorldTileType.Farm:
                    tileAsset = farmTile;
                    break;

                default:
                    tileAsset = null;
                    break;
            }

            return tileAsset != null;
        }

        private void CacheComponents()
        {
            tilemap = gameObject.GetComponentInHierarchy<Tilemap>();
        }
    }
}
