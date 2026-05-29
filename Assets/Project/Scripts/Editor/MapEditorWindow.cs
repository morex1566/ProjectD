using System.Collections.Generic;
using System.Linq;
using TRPG.Runtime;
using UnityEditor;
using UnityEngine;

namespace TRPG.Editor
{
    public class MapEditorWindow : EditorWindow
    {
        private readonly Dictionary<Vector3Int, TileController> workingTiles = new();
        private readonly List<TileController> tilePalette = new();

        private MapData targetMapData = null;
        private Vector2Int origin = new(-3, -3);
        private Vector2Int size = new(7, 7);
        private int selectedPaletteIndex = -1;
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
            }
            EditorGUILayout.EndHorizontal();

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
            if (!workingTiles.TryGetValue(cellPos, out TileController tilePb) || tilePb == null)
            {
                return ".";
            }

            int paletteIndex = tilePalette.IndexOf(tilePb);
            return paletteIndex >= 0 ? paletteIndex.ToString() : "*";
        }

        private void Paint(Vector3Int cellPos)
        {
            if (selectedPaletteIndex < 0)
            {
                workingTiles.Remove(cellPos);
                return;
            }

            if (selectedPaletteIndex >= tilePalette.Count || tilePalette[selectedPaletteIndex] == null)
            {
                return;
            }

            workingTiles[cellPos] = tilePalette[selectedPaletteIndex];
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

            foreach (MapTileData tile in targetMapData.Tiles)
            {
                if (tile.TilePb == null) continue;

                workingTiles[tile.CellPos] = tile.TilePb;
                if (!tilePalette.Contains(tile.TilePb))
                {
                    tilePalette.Add(tile.TilePb);
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

            Undo.RecordObject(targetMapData, "Save Map Data");
            targetMapData.SetTiles(tiles);
            EditorUtility.SetDirty(targetMapData);
            AssetDatabase.SaveAssets();
        }
    }
}
