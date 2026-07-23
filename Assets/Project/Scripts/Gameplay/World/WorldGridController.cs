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
        [SerializeField] private TileBase spawnableTile;
        [SerializeField] private TileBase buildingTile;


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

                if (TryGetTileAsset(worldTile.Flag, out TileBase tileAsset) == false)
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
            if (TryGetTileAsset(worldTile.Flag, out TileBase tileAsset) == false)
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
        /// 타일 플래그에 맞는 시각적 타일 에셋을 반환합니다.
        /// </summary>
        private bool TryGetTileAsset(WorldTileFlag flag, out TileBase tileAsset)
        {
            if ((flag & WorldTileFlag.Gate) != 0)
            {
                tileAsset = gateTile;
                return tileAsset != null;
            }

            if ((flag & WorldTileFlag.Building) != 0)
            {
                tileAsset = buildingTile;
                return tileAsset != null;
            }

            if ((flag & WorldTileFlag.Road) != 0)
            {
                tileAsset = roadTile;
                return tileAsset != null;
            }

            if ((flag & WorldTileFlag.Spawnable) != 0)
            {
                tileAsset = spawnableTile;
                return tileAsset != null;
            }

            tileAsset = null;
            return false;
        }

        private void CacheComponents()
        {
            tilemap = gameObject.GetComponentInHierarchy<Tilemap>();
        }
    }
}
