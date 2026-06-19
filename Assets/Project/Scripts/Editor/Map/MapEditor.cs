using System.IO;
using TRPG.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace TRPG.Editor
{
    /// <summary>
    /// SCN_MapEditor의 Tilemap을 MapData ScriptableObject로 변환합니다.
    /// </summary>
    [CustomEditor(typeof(MapEditorController))]
    public class MapEditor : UnityEditor.Editor
    {
        private const string ScenePath = "Assets/Project/Scenes/SCN_MapEditor.unity";
        private const string CreateSceneMenuPath = "Tools/TRPG/MapController/Create SCN_MapEditor";
        private const string ConvertMenuPath = "Tools/TRPG/MapController/Convert Active MapEditor Tilemap";
        private const string StartSpawnPointName = "Start Spawn Point";

        /// <summary>
        /// 기본 Inspector에 MapData 변환과 시작 스폰 포인트 도구 버튼을 추가합니다.
        /// </summary>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();

            MapEditorController controller = (MapEditorController)target;
            DrawStartSpawnPointTool(controller);

            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(controller.SourceTilemap == null))
            {
                if (GUILayout.Button("Convert Tilemap To MapData"))
                {
                    ConvertToMapData(controller, true);
                }
            }
        }

        /// <summary>
        /// Tilemap, 시작 스폰 포인트, 카메라가 포함된 맵 에디터 씬을 생성합니다.
        /// </summary>
        [MenuItem(CreateSceneMenuPath)]
        public static void CreateMapEditorScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 변환 설정을 들고 있을 컨트롤러와 실제 타일을 칠할 Grid/Tilemap을 구성합니다.
            GameObject mapEditorObj = new GameObject("MapEditor");
            MapEditorController controller = mapEditorObj.AddComponent<MapEditorController>();

            GameObject gridObj = new GameObject("Grid");
            gridObj.AddComponent<Grid>();

            GameObject tilesObj = new GameObject("Tiles");
            tilesObj.transform.SetParent(gridObj.transform);

            Tilemap tilemap = tilesObj.AddComponent<Tilemap>();
            tilesObj.AddComponent<TilemapRenderer>();

            GameObject startSpawnPointObj = new GameObject(StartSpawnPointName);
            startSpawnPointObj.transform.SetParent(gridObj.transform);

            GameObject cameraObj = new GameObject("Main Camera");
            Camera camera = cameraObj.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 8f;
            cameraObj.AddComponent<AudioListener>();
            cameraObj.transform.position = new Vector3(0f, 0f, -10f);
            cameraObj.tag = "MainCamera";

            // 생성한 씬 오브젝트 참조를 SerializedObject로 연결해 private SerializeField 값을 보존합니다.
            SerializedObject serializedController = new SerializedObject(controller);
            serializedController.FindProperty("sourceTilemap").objectReferenceValue = tilemap;
            serializedController.FindProperty("startSpawnPoint").objectReferenceValue = startSpawnPointObj.transform;
            serializedController.ApplyModifiedPropertiesWithoutUndo();

            EnsureFolder(Path.GetDirectoryName(ScenePath)?.Replace('\\', '/'));
            EditorSceneManager.SaveScene(scene, ScenePath);
            Selection.activeObject = controller;

            Debug.Log($"SCN_MapEditor scene created: {ScenePath}");
        }

        /// <summary>
        /// Inspector에서 시작 스폰 포인트를 생성하거나 선택할 수 있는 버튼을 그립니다.
        /// </summary>
        private static void DrawStartSpawnPointTool(MapEditorController controller)
        {
            if (controller.StartSpawnPoint == null)
            {
                if (GUILayout.Button("Create Start Spawn Point"))
                {
                    CreateStartSpawnPoint(controller);
                }

                return;
            }

            if (GUILayout.Button("Select Start Spawn Point"))
            {
                Selection.activeObject = controller.StartSpawnPoint.gameObject;
            }
        }

        /// <summary>
        /// SourceTilemap Grid 아래에 시작 스폰 포인트 Transform을 생성하고 컨트롤러에 연결합니다.
        /// </summary>
        private static void CreateStartSpawnPoint(MapEditorController controller)
        {
            GameObject startSpawnPointObj = new GameObject(StartSpawnPointName);
            Undo.RegisterCreatedObjectUndo(startSpawnPointObj, "Create Start Spawn Point");

            Transform parent = controller.SourceTilemap != null && controller.SourceTilemap.layoutGrid != null
                ? controller.SourceTilemap.layoutGrid.transform
                : controller.transform;
            startSpawnPointObj.transform.SetParent(parent);
            startSpawnPointObj.transform.position = controller.SourceTilemap != null
                ? controller.SourceTilemap.GetCellCenterWorld(Vector3Int.zero)
                : controller.transform.position;

            Undo.RecordObject(controller, "Assign Start Spawn Point");
            SerializedObject serializedController = new SerializedObject(controller);
            serializedController.FindProperty("startSpawnPoint").objectReferenceValue = startSpawnPointObj.transform;
            serializedController.ApplyModifiedProperties();

            Selection.activeObject = startSpawnPointObj;
        }

        /// <summary>
        /// 현재 활성 씬의 MapEditorController를 찾아 Tilemap을 MapData로 변환합니다.
        /// </summary>
        [MenuItem(ConvertMenuPath)]
        public static void ConvertActiveMapEditorTilemap()
        {
            MapEditorController controller = UnityEngine.Object.FindFirstObjectByType<MapEditorController>();
            if (controller == null)
            {
                Debug.LogError("Active scene에 MapEditorSceneController가 없습니다.");
                return;
            }

            ConvertToMapData(controller, true);
        }

        /// <summary>
        /// SourceTilemap의 점유 영역을 읽어 MapData 타일 배열과 시작 스폰 위치로 저장합니다.
        /// </summary>
        private static void ConvertToMapData(MapEditorController controller, bool logResult)
        {
            Tilemap tilemap = controller.SourceTilemap;
            if (tilemap == null)
            {
                Debug.LogError("MapEditorController.SourceTilemap이 비어 있습니다.");
                return;
            }

            if (!TryGetOccupiedBounds(tilemap, out BoundsInt occupiedBounds))
            {
                Debug.LogError("변환할 타일이 없습니다. SourceTilemap에 타일을 먼저 칠해야 합니다.");
                return;
            }

            bool hasStartSpawnPoint = TryGetStartSpawnCell(controller, tilemap, out Vector3Int startSpawnCell);
            if (hasStartSpawnPoint)
            {
                // 스폰 위치가 칠해진 타일 영역 밖에 있어도 MapData 범위에 포함합니다.
                occupiedBounds = EncapsulateCell(occupiedBounds, startSpawnCell);
            }

            MapData mapData = LoadOrCreateMapData(controller);
            Vector3Int pivot = controller.NormalizePivotToZero ? Vector3Int.zero : occupiedBounds.min;
            mapData.Init(occupiedBounds.size.x, occupiedBounds.size.y, occupiedBounds.size.y, pivot);
            if (hasStartSpawnPoint)
            {
                // Tilemap 셀 좌표를 MapData 내부 로컬 좌표로 저장합니다.
                mapData.SetStartSpawnPoint(startSpawnCell - occupiedBounds.min);
            }

            for (int y = occupiedBounds.yMin; y < occupiedBounds.yMax; y++)
            {
                for (int x = occupiedBounds.xMin; x < occupiedBounds.xMax; x++)
                {
                    Vector3Int cellPos = new Vector3Int(x, y, 0);
                    int dataX = x - occupiedBounds.xMin;
                    int dataY = y - occupiedBounds.yMin;
                    MapTileType tileType = GetTileType(controller, tilemap, cellPos);

                    mapData.SetTileType(dataX, dataY, tileType);
                }
            }

            EditorUtility.SetDirty(mapData);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (logResult)
            {
                string assetPath = AssetDatabase.GetAssetPath(mapData);
                Debug.Log($"Tilemap converted to MapData. Path: {assetPath}, Size: {mapData.Width}x{mapData.Height}, StartSpawnPoint: {mapData.StartSpawnPoint}");
            }
        }

        /// <summary>
        /// 컨트롤러에 연결된 MapData를 반환하거나 출력 경로에 새 MapData 에셋을 만듭니다.
        /// </summary>
        private static MapData LoadOrCreateMapData(MapEditorController controller)
        {
            if (controller.TargetMapData != null)
            {
                return controller.TargetMapData;
            }

            string outputFolder = NormalizeAssetFolder(controller.OutputFolder);
            string outputAssetName = NormalizeAssetName(controller.OutputAssetName);
            EnsureFolder(outputFolder);

            string assetPath = $"{outputFolder}/{outputAssetName}";
            MapData mapData = AssetDatabase.LoadAssetAtPath<MapData>(assetPath);
            if (mapData != null)
            {
                return mapData;
            }

            mapData = ScriptableObject.CreateInstance<MapData>();
            AssetDatabase.CreateAsset(mapData, assetPath);

            return mapData;
        }

        /// <summary>
        /// Tilemap에서 실제 타일이 존재하는 최소 Bounds를 계산합니다.
        /// </summary>
        private static bool TryGetOccupiedBounds(Tilemap tilemap, out BoundsInt occupiedBounds)
        {
            BoundsInt cellBounds = tilemap.cellBounds;
            bool hasTile = false;
            Vector3Int min = Vector3Int.zero;
            Vector3Int max = Vector3Int.zero;

            foreach (Vector3Int cellPos in cellBounds.allPositionsWithin)
            {
                if (!tilemap.HasTile(cellPos)) continue;

                if (!hasTile)
                {
                    min = cellPos;
                    max = cellPos;
                    hasTile = true;
                    continue;
                }

                min = Vector3Int.Min(min, cellPos);
                max = Vector3Int.Max(max, cellPos);
            }

            if (!hasTile)
            {
                occupiedBounds = default;
                return false;
            }

            Vector3Int size = max - min + Vector3Int.one;
            occupiedBounds = new BoundsInt(min, size);
            return true;
        }

        /// <summary>
        /// 시작 스폰 Transform의 월드 좌표를 SourceTilemap 셀 좌표로 변환합니다.
        /// </summary>
        private static bool TryGetStartSpawnCell(MapEditorController controller, Tilemap tilemap, out Vector3Int startSpawnCell)
        {
            if (controller.StartSpawnPoint == null)
            {
                startSpawnCell = default;
                Debug.LogWarning("Start Spawn Point가 비어 있어 MapData.StartSpawnPoint는 기본값으로 저장됩니다.");
                return false;
            }

            startSpawnCell = tilemap.WorldToCell(controller.StartSpawnPoint.position);
            startSpawnCell.z = 0;
            return true;
        }

        /// <summary>
        /// 지정 셀이 포함되도록 BoundsInt를 확장합니다.
        /// </summary>
        private static BoundsInt EncapsulateCell(BoundsInt bounds, Vector3Int cellPos)
        {
            Vector3Int min = Vector3Int.Min(bounds.min, cellPos);
            Vector3Int max = Vector3Int.Max(bounds.max - Vector3Int.one, cellPos);

            return new BoundsInt(min, max - min + Vector3Int.one);
        }

        /// <summary>
        /// TileBase와 컨트롤러 설정을 기준으로 MapData에 저장할 타일 타입을 결정합니다.
        /// </summary>
        private static MapTileType GetTileType(MapEditorController controller, Tilemap tilemap, Vector3Int cellPos)
        {
            TileBase tile = tilemap.GetTile(cellPos);
            if (tile == null) return MapTileType.Air;

            if (controller.GroundSurfaceTile != null && tile == controller.GroundSurfaceTile)
            {
                return MapTileType.GroundSurface;
            }

            if (controller.GroundTile != null && tile == controller.GroundTile)
            {
                return MapTileType.Ground;
            }

            if (controller.InferSurfaceTile && !tilemap.HasTile(cellPos + Vector3Int.up))
            {
                return MapTileType.GroundSurface;
            }

            return controller.DefaultSolidTileType;
        }

        /// <summary>
        /// Unity AssetDatabase에서 사용할 폴더 경로를 정규화합니다.
        /// </summary>
        private static string NormalizeAssetFolder(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder))
            {
                return "Assets/Project/Datas/MapController";
            }

            return folder.Replace('\\', '/').TrimEnd('/');
        }

        /// <summary>
        /// 출력 에셋 이름을 파일명으로 정리하고 .asset 확장자를 보장합니다.
        /// </summary>
        private static string NormalizeAssetName(string assetName)
        {
            if (string.IsNullOrWhiteSpace(assetName))
            {
                return "SO_MapEditor.asset";
            }

            assetName = Path.GetFileName(assetName);
            return assetName.EndsWith(".asset") ? assetName : $"{assetName}.asset";
        }

        /// <summary>
        /// AssetDatabase 폴더가 없으면 부모 폴더부터 재귀적으로 생성합니다.
        /// </summary>
        private static void EnsureFolder(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder) || AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
            string name = Path.GetFileName(folder);

            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
