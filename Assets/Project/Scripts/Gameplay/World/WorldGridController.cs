using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TRPG.Runtime
{
    /// <summary>
    /// 맵 런타임 객체와 Unity Tilemap 표현을 연결합니다.
    /// </summary>
    [RequireComponent(typeof(WorldGridContext))]
    public class WorldGridController : MonoBehaviour
    {
        [Header(nameof(WorldGridController))]

        /// <summary>
        /// 런타임 맵 상태입니다.
        /// </summary>
        [SerializeField, ReadOnly] private WorldGridContext gridContext = null;


        [Header("Ground")]

        [SerializeField] private WorldTilemapBrush groundBrush = null;

        [SerializeField] private Vector2Int size = Vector2Int.zero;




        private void OnValidate()
        {
            Init();
        }

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            gridContext = GetComponent<WorldGridContext>();
        }

        /// <summary>
        /// 현재 브러시에서 랜덤 타일을 뽑아 지정 좌표에 그립니다.
        /// </summary>
        public void Draw(WorldTilemapType tilemapType, Vector3Int cellPos)
        {
            if (groundBrush == null)
            {
                return;
            }

            if (groundBrush.TryGetRandomTile(out WorldTile tile) == false)
            {
                return;
            }

            if (gridContext.TilemapContextMap.TryGetValue(tilemapType, out WorldTilemapContext tilemapContext) == false)
            {
                return;
            }

            tile.Pos = cellPos;
            tilemapContext.SetTile(tile);
        }

        [ContextMenu(nameof(DrawGround))]
        public void DrawGround()
        {
            if (gridContext.TilemapContextMap.TryGetValue(WorldTilemapType.WorldTilemapGround, out WorldTilemapContext tilemapContext) == false)
            {
                Debug.LogError($"require {WorldTilemapType.WorldTilemapGround}");
                return;
            }
            else
            {
                tilemapContext.Clear();
            }

            for (int y = 0; y < size.x; y++)
            {
                for (int x = 0; x < size.y; x++)
                {
                    Draw(WorldTilemapType.WorldTilemapGround, new Vector3Int(x, y));
                }
            }
        }
    }
}
