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
        public enum EditLayer
        {
            Tile,
            Monster,
        }


        [Header(nameof(MapEditorSceneController) + ".Setup")]

        [SerializeField] private MapData targetMapData = null;

        [SerializeField] private Transform tileRoot = null;

        [SerializeField] private List<TileController> tilePalette = new();

        [SerializeField] private List<CreatureData> monsterPalette = new();

        [SerializeField] private bool enableScenePaint = true;

        [SerializeField] private EditLayer editLayer = EditLayer.Tile;

        [SerializeField] private bool eraseMode = false;

        [SerializeField] private int selectedPaletteIndex = 0;

        [SerializeField] private int selectedMonsterPaletteIndex = 0;

        [SerializeField] private List<MapMonsterSpawnData> monsterSpawns = new();


        public MapData TargetMapData => targetMapData;

        public Transform TileRoot => tileRoot;

        public IReadOnlyList<TileController> TilePalette => tilePalette;

        public IReadOnlyList<CreatureData> MonsterPalette => monsterPalette;

        public bool EnableScenePaint => enableScenePaint;

        public EditLayer CurrentEditLayer => editLayer;

        public bool EraseMode => eraseMode;

        public int SelectedPaletteIndex => selectedPaletteIndex;

        public int SelectedMonsterPaletteIndex => selectedMonsterPaletteIndex;

        public IReadOnlyList<MapMonsterSpawnData> MonsterSpawns => monsterSpawns;


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

        public void SetMonsterSpawns(IEnumerable<MapMonsterSpawnData> nextMonsterSpawns)
        {
            monsterSpawns.Clear();

            foreach (MapMonsterSpawnData monsterSpawn in nextMonsterSpawns)
            {
                if (!monsterSpawn.HasMonsterDataReference) continue;
                if (targetMapData != null && !targetMapData.HasTile(monsterSpawn.CellPos)) continue;

                monsterSpawns.Add(monsterSpawn);
#if UNITY_EDITOR
                CreatureData editorMonsterData = monsterSpawn.EditorMonsterData;
                if (editorMonsterData != null && !monsterPalette.Contains(editorMonsterData))
                {
                    monsterPalette.Add(editorMonsterData);
                }
#endif
            }
        }

#if UNITY_EDITOR
        public void SetMonsterSpawn(Vector3Int cellPos, CreatureData monsterData)
        {
            RemoveMonsterSpawn(cellPos);
            if (monsterData == null) return;

            monsterSpawns.Add(new MapMonsterSpawnData(cellPos, monsterData));
        }
#endif

        public bool TryGetMonsterSpawn(Vector3Int cellPos, out MapMonsterSpawnData monsterSpawn)
        {
            foreach (MapMonsterSpawnData spawn in monsterSpawns)
            {
                if (spawn.CellPos != cellPos) continue;

                monsterSpawn = spawn;
                return true;
            }

            monsterSpawn = null;
            return false;
        }

        public void RemoveMonsterSpawn(Vector3Int cellPos)
        {
            monsterSpawns.RemoveAll(monsterSpawn => monsterSpawn.CellPos == cellPos);
        }

        public void ClearMonsterSpawns()
        {
            monsterSpawns.Clear();
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
