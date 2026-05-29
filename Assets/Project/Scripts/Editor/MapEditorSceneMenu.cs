using TRPG.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TRPG.Editor
{
    public static class MapEditorSceneMenu
    {
        private const string ScenePath = "Assets/Project/Scenes/SCN_MapEditor.unity";
        private const string DefaultMapDataPath = "Assets/Project/Datas/MapData/SO_Map_Default.asset";
        private const string BlackTilePath = "Assets/Project/Prefabs/Gameplay/PF_Tile_Black.prefab";
        private const string WhiteTilePath = "Assets/Project/Prefabs/Gameplay/PF_Tile_White.prefab";

        [MenuItem("TRPG/Tools/Open Map Editor Scene")]
        private static void OpenMapEditorScene()
        {
            if (!System.IO.File.Exists(ScenePath))
            {
                CreateMapEditorScene();
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        [MenuItem("TRPG/Tools/Recreate Map Editor Scene")]
        private static void CreateMapEditorScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject editorObject = new GameObject("MapEditor");
            MapEditorSceneController controller = editorObject.AddComponent<MapEditorSceneController>();

            GameObject tileRoot = new GameObject("Tiles");
            tileRoot.transform.SetParent(editorObject.transform);
            tileRoot.transform.localPosition = Vector3.zero;

            SerializedObject serializedController = new SerializedObject(controller);
            serializedController.FindProperty("targetMapData").objectReferenceValue = AssetDatabase.LoadAssetAtPath<MapData>(DefaultMapDataPath);
            serializedController.FindProperty("tileRoot").objectReferenceValue = tileRoot.transform;
            serializedController.FindProperty("enableScenePaint").boolValue = true;
            serializedController.FindProperty("eraseMode").boolValue = false;
            serializedController.FindProperty("selectedPaletteIndex").intValue = 0;

            SerializedProperty paletteProperty = serializedController.FindProperty("tilePalette");
            paletteProperty.arraySize = 2;
            paletteProperty.GetArrayElementAtIndex(0).objectReferenceValue = LoadTilePrefab(BlackTilePath);
            paletteProperty.GetArrayElementAtIndex(1).objectReferenceValue = LoadTilePrefab(WhiteTilePath);
            serializedController.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            Selection.activeObject = controller;
        }

        private static TileController LoadTilePrefab(string path)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            return prefab != null ? prefab.GetComponent<TileController>() : null;
        }
    }
}
