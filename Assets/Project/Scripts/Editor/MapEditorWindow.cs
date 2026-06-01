using System.Collections.Generic;
using System.Linq;
using TRPG.Runtime;
using UnityEditor;
using UnityEngine;

namespace TRPG.Editor
{
    public class MapEditorWindow : EditorWindow
    {
        private enum EditLayer
        {
            Tile,
            Monster,
        }


        private readonly Dictionary<Vector3Int, TileController> workingTiles = new();
        private readonly Dictionary<Vector3Int, CreatureData> workingMonsterSpawns = new();
        private readonly List<TileController> tilePalette = new();
        private readonly List<CreatureData> monsterPalette = new();

        private MapData targetMapData = null;
        private EditLayer editLayer = EditLayer.Tile;
        private Vector2Int origin = new(-3, -3);
        private Vector2Int size = new(7, 7);
        private int selectedPaletteIndex = -1;
        private int selectedMonsterPaletteIndex = -1;
        private Vector2 scrollPos = Vector2.zero;

        [MenuItem("TRPG/Tools/Map Editor")]
        private static void Open()
        {
            GetWindow<MapEditorWindow>("Map Editor");
        }

        private void OnGUI()
        {
            DrawTargetControls();
            DrawPaletteControls();
            DrawMonsterPaletteControls();
            DrawMapGrid();
        }

        private void DrawTargetControls()
        {
            EditorGUILayout.LabelField("Map Data", EditorStyles.boldLabel);
            targetMapData = (MapData)EditorGUILayout.ObjectField("Target", targetMapData, typeof(MapData), false);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create New"))
            {
                CreateNewMapData();
            }

            using (new EditorGUI.DisabledScope(targetMapData == null))
            {
                if (GUILayout.Button("Load"))
                {
                    LoadFromTarget();
                }

                if (GUILayout.Button("Save"))
                {
                    SaveToTarget();
                }
            }

            if (GUILayout.Button("Clear"))
            {
                workingTiles.Clear();
                workingMonsterSpawns.Clear();
            }
            EditorGUILayout.EndHorizontal();

            editLayer = (EditLayer)EditorGUILayout.EnumPopup("Edit Layer", editLayer);
            origin = EditorGUILayout.Vector2IntField("Origin", origin);
            size = EditorGUILayout.Vector2IntField("Size", size);
            size.x = Mathf.Max(1, size.x);
            size.y = Mathf.Max(1, size.y);
        }

        private void DrawPaletteControls()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Tile Palette", EditorStyles.boldLabel);

            if (GUILayout.Toggle(selectedPaletteIndex == -1, "Erase", "Button"))
            {
                selectedPaletteIndex = -1;
            }

            for (int i = 0; i < tilePalette.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Toggle(selectedPaletteIndex == i, $"#{i}", "Button", GUILayout.Width(44)))
                {
                    selectedPaletteIndex = i;
                }

                tilePalette[i] = (TileController)EditorGUILayout.ObjectField(tilePalette[i], typeof(TileController), false);

                if (GUILayout.Button("-", GUILayout.Width(28)))
                {
                    tilePalette.RemoveAt(i);
                    if (selectedPaletteIndex >= tilePalette.Count) selectedPaletteIndex = -1;
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add Tile Prefab"))
            {
                tilePalette.Add(null);
            }
        }

        private void DrawMonsterPaletteControls()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Monster Palette", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(editLayer != EditLayer.Monster))
            {
                if (GUILayout.Toggle(selectedMonsterPaletteIndex == -1, "Erase Monster", "Button"))
                {
                    selectedMonsterPaletteIndex = -1;
                }

                for (int i = 0; i < monsterPalette.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    string monsterName = monsterPalette[i] != null ? monsterPalette[i].name : "None";
                    if (GUILayout.Toggle(selectedMonsterPaletteIndex == i, $"#{i} {monsterName}", "Button", GUILayout.Width(120)))
                    {
                        selectedMonsterPaletteIndex = i;
                    }

                    monsterPalette[i] = (CreatureData)EditorGUILayout.ObjectField(monsterPalette[i], typeof(CreatureData), false);

                    if (GUILayout.Button("-", GUILayout.Width(28)))
                    {
                        monsterPalette.RemoveAt(i);
                        if (selectedMonsterPaletteIndex >= monsterPalette.Count) selectedMonsterPaletteIndex = -1;
                        break;
                    }
                    EditorGUILayout.EndHorizontal();
                }

                if (GUILayout.Button("Add Monster Data"))
                {
                    monsterPalette.Add(null);
                }
            }
        }

        private void DrawMapGrid()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Grid", EditorStyles.boldLabel);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            for (int y = origin.y + size.y - 1; y >= origin.y; y--)
            {
                EditorGUILayout.BeginHorizontal();
                for (int x = origin.x; x < origin.x + size.x; x++)
                {
                    Vector3Int cellPos = new(x, y, 0);
                    string label = GetTileLabel(cellPos);
                    if (GUILayout.Button(label, GUILayout.Width(40), GUILayout.Height(32)))
                    {
                        Paint(cellPos);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        private string GetTileLabel(Vector3Int cellPos)
        {
            string tileLabel = ".";
            if (workingTiles.TryGetValue(cellPos, out TileController tilePb) && tilePb != null)
            {
                int paletteIndex = tilePalette.IndexOf(tilePb);
                tileLabel = paletteIndex >= 0 ? paletteIndex.ToString() : "*";
            }

            if (!workingMonsterSpawns.TryGetValue(cellPos, out CreatureData monsterData) || monsterData == null)
            {
                return tileLabel;
            }

            int monsterPaletteIndex = monsterPalette.IndexOf(monsterData);
            string monsterLabel = monsterPaletteIndex >= 0 ? $"M{monsterPaletteIndex}" : "M*";
            return tileLabel == "." ? monsterLabel : $"{tileLabel}/{monsterLabel}";
        }

        private void Paint(Vector3Int cellPos)
        {
            if (editLayer == EditLayer.Monster)
            {
                PaintMonster(cellPos);
                return;
            }

            PaintTile(cellPos);
        }

        private void PaintTile(Vector3Int cellPos)
        {
            if (selectedPaletteIndex < 0)
            {
                workingTiles.Remove(cellPos);
                workingMonsterSpawns.Remove(cellPos);
                return;
            }

            if (selectedPaletteIndex >= tilePalette.Count || tilePalette[selectedPaletteIndex] == null)
            {
                return;
            }

            workingTiles[cellPos] = tilePalette[selectedPaletteIndex];
        }

        private void PaintMonster(Vector3Int cellPos)
        {
            if (selectedMonsterPaletteIndex < 0)
            {
                workingMonsterSpawns.Remove(cellPos);
                return;
            }

            if (!workingTiles.ContainsKey(cellPos))
            {
                Debug.LogWarning($"Paint failed. 몬스터 스폰은 타일이 있는 CellPos에만 배치할 수 있습니다. CellPos: {cellPos}");
                return;
            }

            if (selectedMonsterPaletteIndex >= monsterPalette.Count || monsterPalette[selectedMonsterPaletteIndex] == null)
            {
                return;
            }

            workingMonsterSpawns[cellPos] = monsterPalette[selectedMonsterPaletteIndex];
        }

        private void CreateNewMapData()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Create Map Data",
                "SO_Map_New",
                "asset",
                "MapData asset을 저장할 위치를 선택하세요.",
                "Assets/Project/Datas");

            if (string.IsNullOrEmpty(path)) return;

            targetMapData = CreateInstance<MapData>();
            AssetDatabase.CreateAsset(targetMapData, path);
            SaveToTarget();
            Selection.activeObject = targetMapData;
        }

        private void LoadFromTarget()
        {
            workingTiles.Clear();
            workingMonsterSpawns.Clear();

            foreach (MapTileData tile in targetMapData.Tiles)
            {
                if (tile.TilePb == null) continue;

                workingTiles[tile.CellPos] = tile.TilePb;
                if (!tilePalette.Contains(tile.TilePb))
                {
                    tilePalette.Add(tile.TilePb);
                }
            }

            foreach (MapMonsterSpawnData monsterSpawn in targetMapData.MonsterSpawns)
            {
                CreatureData editorMonsterData = monsterSpawn.EditorMonsterData;
                if (editorMonsterData == null) continue;
                if (!targetMapData.HasTile(monsterSpawn.CellPos)) continue;

                workingMonsterSpawns[monsterSpawn.CellPos] = editorMonsterData;
                if (!monsterPalette.Contains(editorMonsterData))
                {
                    monsterPalette.Add(editorMonsterData);
                }
            }

            BoundsInt bounds = targetMapData.GetBounds();
            if (bounds.size != Vector3Int.zero)
            {
                origin = new Vector2Int(bounds.min.x, bounds.min.y);
                size = new Vector2Int(bounds.size.x, bounds.size.y);
            }
        }

        private void SaveToTarget()
        {
            if (targetMapData == null) return;

            IEnumerable<MapTileData> tiles = workingTiles
                .Where(pair => pair.Value != null)
                .OrderBy(pair => pair.Key.y)
                .ThenBy(pair => pair.Key.x)
                .Select(pair => new MapTileData(pair.Key, pair.Value));

            IEnumerable<MapMonsterSpawnData> monsterSpawns = workingMonsterSpawns
                .Where(pair => pair.Value != null)
                .Where(pair => workingTiles.ContainsKey(pair.Key))
                .OrderBy(pair => pair.Key.y)
                .ThenBy(pair => pair.Key.x)
                .Select(pair => new MapMonsterSpawnData(pair.Key, pair.Value));

            Undo.RecordObject(targetMapData, "Save Map Data");
            targetMapData.SetTiles(tiles);
            targetMapData.SetMonsterSpawns(monsterSpawns);
            EditorUtility.SetDirty(targetMapData);
            AssetDatabase.SaveAssets();
        }
    }
}
