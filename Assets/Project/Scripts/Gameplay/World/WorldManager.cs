using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TRPG.Runtime
{
    public class WorldManager : MonoBehaviourSingleton<WorldManager>
    {
        [SerializeField] private List<WorldTile> tiles = new List<WorldTile>();

        private static WorldGridController gridController = null;

        private static WorldCameraContorller cameraController = null;

        public IReadOnlyList<WorldTile> Tiles => tiles;


        private void Awake()
        {
            CacheComponents();    
        }

        [MenuItem("Tools/" + nameof(StartLoop))]
        public static void StartLoop()
        {
            
        }

        public static void SetTiles(IReadOnlyList<WorldTile> worldTiles)
        {
            gridController.SetTiles(worldTiles);
        }

        public static void SetTile(WorldTile worldTile)
        {
            gridController.SetTile(worldTile);
        }

        /// <summary>
        /// 현재 마우스 위치에 해당하는 Hex 타일 셀 좌표를 반환합니다.
        /// </summary>
        public static bool TryGetMouseCellPosition(out Vector3Int cellPosition)
        {
            cellPosition = default;

            if (MouseEx.TryGetMouseWorldPosition(cameraController.Cam, out Vector3 mouseWorldPosition) == false)
            {
                return false;
            }

            cellPosition = gridController.WorldToCell(mouseWorldPosition);
            return true;
        }

        private void CacheComponents()
        {
            gridController = GameObject.FindWithTag(UnityConstant.Tags.WorldGridController).GetComponentInHierarchy<WorldGridController>();
            cameraController = GameObject.FindWithTag(UnityConstant.Tags.WorldCamera).GetComponentInHierarchy<WorldCameraContorller>();
        }
    }
}
