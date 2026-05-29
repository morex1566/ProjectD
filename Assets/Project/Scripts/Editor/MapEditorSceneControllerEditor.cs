using System.Collections.Generic;
using System.Linq;
using TRPG.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TRPG.Editor
{
    [CustomEditor(typeof(MapEditorSceneController))]
    public class MapEditorSceneControllerEditor : UnityEditor.Editor
    {
        private SerializedProperty enableScenePaintProperty = null;
        private SerializedProperty eraseModeProperty = null;
        private SerializedProperty selectedPaletteIndexProperty = null;

        private Vector3Int? lastPaintedCellPos = null;

        private MapEditorSceneController Controller => (MapEditorSceneController)target;

        private void OnEnable()
        {
            enableScenePaintProperty = serializedObject.FindProperty("enableScenePaint");
            eraseModeProperty = serializedObject.FindProperty("eraseMode");
            selectedPaletteIndexProperty = serializedObject.FindProperty("selectedPaletteIndex");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script", "enableScenePaint", "eraseMode", "selectedPaletteIndex");
            DrawSceneBrushControls();
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Scene 뷰에서 마우스 위치의 CellPos 인디케이터를 확인하고 좌클릭/드래그로 타일을 배치하거나 Erase로 삭제합니다. Save To MapData를 눌러 ScriptableObject에 기록합니다.",
                MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Ensure Tile Root"))
                {
                    EnsureTileRoot();
                }

                if (GUILayout.Button("Snap Tiles"))
                {
                    SnapTiles();
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Load From MapData"))
                {
                    LoadFromMapData();
                }

                if (GUILayout.Button("Save To MapData"))
                {
                    SaveToMapData();
                }
            }

            if (GUILayout.Button("Clear Scene Tiles"))
            {
                ClearSceneTilesWithConfirm();
            }
        }

        private void OnSceneGUI()
        {
            DrawCellLabels();

            if (!Controller.EnableScenePaint) return;

            Event currentEvent = Event.current;
            if (currentEvent == null) return;

            if (currentEvent.rawType == EventType.MouseUp || currentEvent.type == EventType.MouseLeaveWindow)
            {
                lastPaintedCellPos = null;
            }

            if (!TryGetMouseCellPos(currentEvent, out Vector3Int cellPos)) return;

            DrawPaintIndicator(cellPos);

            int controlId = GUIUtility.GetControlID(FocusType.Passive);
            if (currentEvent.type == EventType.Layout)
            {
                HandleUtility.AddDefaultControl(controlId);
            }

            if (!ShouldPaint(currentEvent, cellPos)) return;

            PaintCell(cellPos);
            lastPaintedCellPos = cellPos;
            currentEvent.Use();
        }

        private void DrawSceneBrushControls()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Scene Brush", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(enableScenePaintProperty, new GUIContent("Enable Scene Paint"));

            using (new EditorGUI.DisabledScope(!enableScenePaintProperty.boolValue))
            {
                bool eraseSelected = eraseModeProperty.boolValue;
                if (GUILayout.Toggle(eraseSelected, "Erase", "Button") != eraseSelected)
                {
                    eraseModeProperty.boolValue = true;
                    selectedPaletteIndexProperty.intValue = -1;
                }

                for (int i = 0; i < Controller.TilePalette.Count; i++)
                {
                    TileController tilePb = Controller.TilePalette[i];
                    string tileName = tilePb != null ? tilePb.name : "None";
                    bool selected = !eraseModeProperty.boolValue && selectedPaletteIndexProperty.intValue == i;

                    if (GUILayout.Toggle(selected, $"#{i} {tileName}", "Button") == selected) continue;

                    eraseModeProperty.boolValue = false;
                    selectedPaletteIndexProperty.intValue = i;
                }
            }

            if (!eraseModeProperty.boolValue &&
                (selectedPaletteIndexProperty.intValue < 0 || selectedPaletteIndexProperty.intValue >= Controller.TilePalette.Count))
            {
                EditorGUILayout.HelpBox("배치할 타일 브러시가 선택되지 않았습니다.", MessageType.Warning);
            }
        }

        private void EnsureTileRoot()
        {
            bool hasExistingRoot = Controller.TileRoot != null || Controller.transform.Find("Tiles") != null;

            Undo.RecordObject(Controller, "Ensure Tile Root");
            Transform root = Controller.EnsureTileRoot();
            if (!hasExistingRoot)
            {
                Undo.RegisterCreatedObjectUndo(root.gameObject, "Create Tile Root");
            }
            EditorUtility.SetDirty(Controller);
            MarkSceneDirty();
        }

        private void LoadFromMapData()
        {
            if (Controller.TargetMapData == null)
            {
                Debug.LogWarning("Load From MapData failed. Target MapData가 없습니다.");
                return;
            }

            if (Controller.TileRoot != null && Controller.TileRoot.childCount > 0)
            {
                bool confirmed = EditorUtility.DisplayDialog(
                    "Load From MapData",
                    "현재 Scene의 Tiles 하위 오브젝트를 지우고 MapData를 다시 로드합니다.",
                    "Load",
                    "Cancel");

                if (!confirmed) return;
            }

            ClearSceneTiles();
            Transform root = Controller.EnsureTileRoot();

            foreach (MapTileData tileData in Controller.TargetMapData.Tiles)
            {
                if (tileData.TilePb == null)
                {
                    Debug.LogWarning($"Load skipped. Tile prefab is null. CellPos: {tileData.CellPos}");
                    continue;
                }

                GameObject tileObject = PrefabUtility.InstantiatePrefab(tileData.TilePb.gameObject, root) as GameObject;
                if (tileObject == null)
                {
                    Debug.LogWarning($"Load skipped. Prefab instantiate failed. Prefab: {tileData.TilePb.name}");
                    continue;
                }

                Undo.RegisterCreatedObjectUndo(tileObject, "Load Map Tile");
                tileObject.transform.position = MapEditorSceneController.CellPosToWorldPosition(tileData.CellPos);
                tileObject.name = $"{tileData.TilePb.name}_{tileData.CellPos.x}_{tileData.CellPos.y}_{tileData.CellPos.z}";
            }

            ApplyTileOrderInLayer();
            MarkSceneDirty();
        }

        private void SaveToMapData()
        {
            if (Controller.TargetMapData == null)
            {
                Debug.LogWarning("Save To MapData failed. Target MapData가 없습니다.");
                return;
            }

            Dictionary<Vector3Int, MapTileData> tilesByCellPos = new();
            foreach (TileController tile in Controller.GetSceneTiles())
            {
                if (!TryGetPrefabAsset(tile, out TileController tilePb))
                {
                    Debug.LogWarning($"Save skipped. Prefab 원본을 찾을 수 없습니다. Tile: {tile.name}");
                    continue;
                }

                Vector3Int cellPos = MapEditorSceneController.WorldPositionToCellPos(tile.transform.position);
                if (tilesByCellPos.ContainsKey(cellPos))
                {
                    Debug.LogWarning($"Save overwrite. 같은 CellPos에 타일이 여러 개 있습니다. CellPos: {cellPos}");
                }

                tilesByCellPos[cellPos] = new MapTileData(cellPos, tilePb);
                Undo.RecordObject(tile.transform, "Snap Tile Before Save");
                tile.transform.position = MapEditorSceneController.CellPosToWorldPosition(cellPos);
                tile.name = $"{tilePb.name}_{cellPos.x}_{cellPos.y}_{cellPos.z}";
            }

            ApplyTileOrderInLayer();

            IEnumerable<MapTileData> orderedTiles = tilesByCellPos
                .OrderBy(pair => pair.Key.y)
                .ThenBy(pair => pair.Key.x)
                .Select(pair => pair.Value);

            Undo.RecordObject(Controller.TargetMapData, "Save Scene Map Data");
            Controller.TargetMapData.SetTiles(orderedTiles);
            EditorUtility.SetDirty(Controller.TargetMapData);
            AssetDatabase.SaveAssets();
            MarkSceneDirty();

            Debug.Log($"Save To MapData complete. Tile Count: {tilesByCellPos.Count}");
        }

        private void SnapTiles()
        {
            foreach (TileController tile in Controller.GetSceneTiles())
            {
                Vector3Int cellPos = MapEditorSceneController.WorldPositionToCellPos(tile.transform.position);
                Undo.RecordObject(tile.transform, "Snap Tile");
                tile.transform.position = MapEditorSceneController.CellPosToWorldPosition(cellPos);
                tile.name = $"{GetPrefabName(tile)}_{cellPos.x}_{cellPos.y}_{cellPos.z}";
            }

            ApplyTileOrderInLayer();
            MarkSceneDirty();
        }

        private void ClearSceneTilesWithConfirm()
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "Clear Scene Tiles",
                "현재 Scene의 Tiles 하위 타일 오브젝트를 모두 삭제합니다. MapData 에셋은 Save 전까지 바뀌지 않습니다.",
                "Clear",
                "Cancel");

            if (!confirmed) return;

            ClearSceneTiles();
            MarkSceneDirty();
        }

        private void ClearSceneTiles()
        {
            Transform root = Controller.EnsureTileRoot();
            List<GameObject> children = new();
            foreach (Transform child in root)
            {
                children.Add(child.gameObject);
            }

            foreach (GameObject child in children)
            {
                Undo.DestroyObjectImmediate(child);
            }
        }

        private void PaintCell(Vector3Int cellPos)
        {
            if (Controller.EraseMode || Controller.SelectedPaletteIndex < 0)
            {
                EraseTileAtCellPos(cellPos, true);
                return;
            }

            if (Controller.SelectedPaletteIndex >= Controller.TilePalette.Count)
            {
                Debug.LogWarning("Paint failed. 선택된 타일 브러시가 Tile Palette 범위를 벗어났습니다.");
                return;
            }

            TileController tilePb = Controller.TilePalette[Controller.SelectedPaletteIndex];
            if (tilePb == null)
            {
                Debug.LogWarning("Paint failed. 선택된 타일 브러시가 비어 있습니다.");
                return;
            }

            EraseTileAtCellPos(cellPos, false);

            Transform root = Controller.EnsureTileRoot();
            GameObject tileObject = PrefabUtility.InstantiatePrefab(tilePb.gameObject, root) as GameObject;
            if (tileObject == null)
            {
                Debug.LogWarning($"Paint failed. Prefab instantiate failed. Prefab: {tilePb.name}");
                return;
            }

            Undo.RegisterCreatedObjectUndo(tileObject, "Paint Map Tile");
            tileObject.transform.position = MapEditorSceneController.CellPosToWorldPosition(cellPos);
            tileObject.name = $"{tilePb.name}_{cellPos.x}_{cellPos.y}_{cellPos.z}";

            ApplyTileOrderInLayer();
            MarkSceneDirty();
        }

        private void EraseTileAtCellPos(Vector3Int cellPos, bool applyAfterErase)
        {
            bool removed = false;
            foreach (TileController tile in Controller.GetSceneTiles())
            {
                Vector3Int tileCellPos = MapEditorSceneController.WorldPositionToCellPos(tile.transform.position);
                if (tileCellPos != cellPos) continue;

                Undo.DestroyObjectImmediate(tile.gameObject);
                removed = true;
            }

            if (!removed || !applyAfterErase) return;

            ApplyTileOrderInLayer();
            MarkSceneDirty();
        }

        private bool TryGetPrefabAsset(TileController sceneTile, out TileController tilePb)
        {
            tilePb = PrefabUtility.GetCorrespondingObjectFromOriginalSource(sceneTile);
            if (tilePb != null) return true;

            GameObject prefabRoot = PrefabUtility.GetCorrespondingObjectFromOriginalSource(sceneTile.gameObject);
            if (prefabRoot == null) return false;

            tilePb = prefabRoot.GetComponent<TileController>();
            return tilePb != null;
        }

        private string GetPrefabName(TileController sceneTile)
        {
            return TryGetPrefabAsset(sceneTile, out TileController tilePb) ? tilePb.name : sceneTile.name;
        }

        private bool TryGetMouseCellPos(Event currentEvent, out Vector3Int cellPos)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
            Plane groundPlane = new Plane(Vector3.forward, Vector3.zero);
            if (!groundPlane.Raycast(ray, out float distance))
            {
                cellPos = Vector3Int.zero;
                return false;
            }

            cellPos = MapEditorSceneController.WorldPositionToCellPos(ray.GetPoint(distance));
            return true;
        }

        private bool ShouldPaint(Event currentEvent, Vector3Int cellPos)
        {
            if (currentEvent.alt) return false;
            if (currentEvent.button != 0) return false;
            if (currentEvent.type != EventType.MouseDown && currentEvent.type != EventType.MouseDrag) return false;
            if (lastPaintedCellPos.HasValue && lastPaintedCellPos.Value == cellPos) return false;

            return true;
        }

        private void DrawCellLabels()
        {
            if (Controller.TileRoot == null) return;

            foreach (TileController tile in Controller.GetSceneTiles())
            {
                Vector3Int cellPos = MapEditorSceneController.WorldPositionToCellPos(tile.transform.position);
                Vector3 worldPosition = MapEditorSceneController.CellPosToWorldPosition(cellPos);

                Handles.color = Color.cyan;
                Handles.DrawWireCube(worldPosition, Vector3.one);
                Handles.Label(worldPosition + Vector3.up * 0.45f, $"CellPos {cellPos.x}, {cellPos.y}");
            }
        }

        private void DrawPaintIndicator(Vector3Int cellPos)
        {
            Vector3 worldPosition = MapEditorSceneController.CellPosToWorldPosition(cellPos);
            Vector3[] vertices =
            {
                worldPosition + new Vector3(-0.5f, -0.5f, 0f),
                worldPosition + new Vector3(-0.5f, 0.5f, 0f),
                worldPosition + new Vector3(0.5f, 0.5f, 0f),
                worldPosition + new Vector3(0.5f, -0.5f, 0f),
            };

            Color fillColor = Controller.EraseMode ? new Color(1f, 0.2f, 0.2f, 0.12f) : new Color(0.1f, 0.75f, 1f, 0.14f);
            Color outlineColor = Controller.EraseMode ? new Color(1f, 0.2f, 0.2f, 0.95f) : new Color(0.1f, 0.75f, 1f, 0.95f);

            Handles.DrawSolidRectangleWithOutline(vertices, fillColor, outlineColor);
            Handles.Label(worldPosition + Vector3.up * 0.65f, $"CellPos {cellPos.x}, {cellPos.y}");
        }

        private void ApplyTileOrderInLayer()
        {
            TileController[] sceneTiles = Controller.GetSceneTiles();
            if (sceneTiles.Length == 0) return;

            int topRowCellY = sceneTiles
                .Select(tile => MapEditorSceneController.WorldPositionToCellPos(tile.transform.position).y)
                .Max();

            foreach (TileController tile in sceneTiles)
            {
                Vector3Int cellPos = MapEditorSceneController.WorldPositionToCellPos(tile.transform.position);
                // CellPos y가 큰 최상단 행부터 SpriteRenderer Order in Layer를 0, 1, 2...로 배정합니다.
                ApplyTileOrderInLayer(tile, topRowCellY - cellPos.y);
            }
        }

        private void ApplyTileOrderInLayer(TileController tile, int baseOrderInLayer)
        {
            SpriteRenderer[] renderers = tile.GetComponentsInChildren<SpriteRenderer>(true);
            if (renderers.Length == 0) return;

            int minOrderInLayer = renderers.Min(renderer => renderer.sortingOrder);
            foreach (SpriteRenderer renderer in renderers)
            {
                int relativeOrderInLayer = renderer.sortingOrder - minOrderInLayer;
                Undo.RecordObject(renderer, "Apply Tile Order In Layer");
                renderer.sortingOrder = baseOrderInLayer + relativeOrderInLayer;
                EditorUtility.SetDirty(renderer);
            }
        }

        private void MarkSceneDirty()
        {
            EditorSceneManager.MarkSceneDirty(Controller.gameObject.scene);
        }
    }
}
