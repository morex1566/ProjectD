using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// SCN_MapEditor에서 실제 타일 프리팹을 배치하고 MapData와 동기화하기 위한 씬 전용 컨트롤러입니다.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class MapEditorSceneController : MonoBehaviour
    {
        [Header(nameof(MapEditorSceneController) + ".Setup")]

        [SerializeField] private MapData targetMapData = null;

        [SerializeField] private Transform tileRoot = null;

        [SerializeField] private List<TileController> tilePalette = new();

        [SerializeField] private bool enableScenePaint = true;

        [SerializeField] private bool eraseMode = false;

        [SerializeField] private int selectedPaletteIndex = 0;


        public MapData TargetMapData => targetMapData;

        public Transform TileRoot => tileRoot;

        public IReadOnlyList<TileController> TilePalette => tilePalette;

        public bool EnableScenePaint => enableScenePaint;

        public bool EraseMode => eraseMode;

        public int SelectedPaletteIndex => selectedPaletteIndex;


        public Transform EnsureTileRoot()
        {
            if (tileRoot != null) return tileRoot;

            Transform existingRoot = transform.Find("Tiles");
            if (existingRoot != null)
            {
                tileRoot = existingRoot;
                return tileRoot;
            }

            GameObject rootObject = new GameObject("Tiles");
            rootObject.transform.SetParent(transform);
            rootObject.transform.localPosition = Vector3.zero;
            tileRoot = rootObject.transform;

            return tileRoot;
        }

        public TileController[] GetSceneTiles()
        {
            Transform root = EnsureTileRoot();

            return root.GetComponentsInChildren<TileController>();
        }

        public static Vector3Int WorldPositionToCellPos(Vector3 worldPosition)
        {
            // 타일 크기는 1입니다. WorldPosition (0, 0, 0)은 CellPos (0, 0)에 매핑되고 z는 논리 좌표에서 사용하지 않습니다.
            return new Vector3Int(
                Mathf.FloorToInt(worldPosition.x + 0.5f),
                Mathf.FloorToInt(worldPosition.y + 0.5f),
                0);
        }

        public static Vector3 CellPosToWorldPosition(Vector3Int cellPos)
        {
            return new Vector3(cellPos.x, cellPos.y, cellPos.z);
        }
    }
}
