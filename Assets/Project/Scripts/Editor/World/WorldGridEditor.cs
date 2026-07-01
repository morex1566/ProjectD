#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using TRPG.Runtime;

namespace TRPG.Editor
{
    /// <summary>
    /// 새 게임 시작 시 사용할 맵 데이터를 생성합니다.
    /// </summary>
    [ExecuteAlways]
    public class WorldGridEditor : MonoBehaviour
    {
        [SerializeField] private GameObject mapPf;

        [SerializeField] private WorldGridData mapData;

        [SerializeField] private WorldTilemapBrush currBrush;

        [SerializeField, HideInInspector] private bool isEditMode;

        private WorldGridContext mapContext;

        private GameObject loadedMap;

        private Vector3Int lastPaintCellPos;

        private WorldTilemapType lastPaintTilemapType = WorldTilemapType.None;

        private int lastPaintButton = -1;

        private bool hasPaintChanges;

        public bool IsEditMode => isEditMode;



        //private void OnEnable()
        //{
        //    SceneView.duringSceneGui += Paint;
        //}

        //private void OnDisable()
        //{
        //    SceneView.duringSceneGui -= Paint;
        //}

        //private void OnTransformChildrenChanged()
        //{
            
        //}


        ///// <summary>
        ///// MapData를 기반으로 PF_Map과 같은 Grid/Tilemap 계층을 생성합니다.
        ///// </summary>
        //[ContextMenu("Load")]
        //public void Load()
        //{
        //    if (mapData == null)
        //    {
        //        Debug.LogWarning($"{nameof(WorldGridEditor)}에 {nameof(mapData)}가 지정되지 않았습니다.", this);
        //        return;
        //    }

        //    // 초기화
        //    ClearLoadedMap();

        //    // 타일맵 추가
        //    loadedMap = Instantiate(mapPf, transform);
        //    foreach (KeyValuePair<WorldTilemapType, WorldTilemapData> pair in mapData.TilemapDatas)
        //    {
        //        if (pair.Key == WorldTilemapType.None || pair.Value == null)
        //        {
        //            continue;
        //        }

        //        Tilemap tilemap = CreateTilemap(pair.Key);
        //        DrawTilemap(tilemap, pair.Value);
        //    }
        //}

        //[ContextMenu("Save")]
        //public void Save()
        //{
        //    if (mapData == null)
        //    {
        //        Debug.LogWarning($"{nameof(WorldGridEditor)}에 {nameof(mapData)}가 지정되지 않았습니다.", this);
        //        return;
        //    }

        //    if (loadedMap == null)
        //    {
        //        Debug.LogWarning("저장할 맵이 로드되지 않았습니다.", this);
        //        return;
        //    }

        //    Dictionary<WorldTilemapType, WorldTilemapData> tilemapDatas = mapData.TilemapDatas;
        //    Tilemap[] tilemaps = loadedMap.GetComponentsInChildren<Tilemap>();

        //    for (int i = 0; i < tilemaps.Length; i++)
        //    {
        //        Tilemap tilemap = tilemaps[i];
        //        if (System.Enum.TryParse(tilemap.name, out WorldTilemapType tilemapType) == false)
        //        {
        //            continue;
        //        }

        //        if (tilemapType == WorldTilemapType.None || tilemapDatas.TryGetValue(tilemapType, out WorldTilemapData tilemapData) == false)
        //        {
        //            continue;
        //        }

        //        SaveTilemap(tilemap, tilemapData);
        //    }

        //    EditorUtility.SetDirty(mapData);
        //    AssetDatabase.SaveAssets();
        //}

        //public void CreateTilemap()
        //{

        //}

        ///// <summary>
        ///// SceneView에서 페인트 입력을 받을지 설정합니다.
        ///// </summary>
        //public void SetEditMode(bool value)
        //{
        //    if (isEditMode == value)
        //    {
        //        return;
        //    }

        //    isEditMode = value;
        //    ResetPaintStroke();
        //    EditorUtility.SetDirty(this);
        //    SceneView.RepaintAll();
        //}

        //public void Paint(SceneView sceneView)
        //{
        //    Event evt = Event.current;
        //    if (evt == null)
        //    {
        //        return;
        //    }

        //    if (evt.type == EventType.Layout && IsPaintReady())
        //    {
        //        // SceneView가 클릭으로 오브젝트를 선택하지 않고 페인트 입력을 받도록 합니다.
        //        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        //        return;
        //    }

        //    if (evt.type == EventType.MouseUp || evt.type == EventType.MouseLeaveWindow)
        //    {
        //        ResetPaintStroke();
        //        return;
        //    }

        //    if (IsPaintEvent(evt) == false)
        //    {
        //        return;
        //    }

        //    if (TryGetPaintTarget(evt, out Tilemap tilemap, out WorldTilemapData tilemapData, out WorldTilemapType tilemapType, out Vector3Int cellPos) == false)
        //    {
        //        return;
        //    }

        //    if (IsSamePaintStroke(evt.button, tilemapType, cellPos))
        //    {
        //        evt.Use();
        //        return;
        //    }

        //    bool changed = evt.button == 0 ? PaintCell(tilemap, tilemapData, cellPos) : EraseCell(tilemap, tilemapData, cellPos);

        //    lastPaintButton = evt.button;
        //    lastPaintTilemapType = tilemapType;
        //    lastPaintCellPos = cellPos;

        //    if (changed)
        //    {
        //        hasPaintChanges = true;
        //        sceneView.Repaint();
        //    }

        //    evt.Use();
        //}

        ///// <summary>
        ///// SceneView 페인트 입력을 처리할 수 있는 기본 상태인지 확인합니다.
        ///// </summary>
        //private bool IsPaintReady()
        //{
        //    return isEditMode && mapData != null && currBrush != null && loadedMap != null;
        //}

        ///// <summary>
        ///// 현재 Event가 맵 페인트 입력인지 확인합니다.
        ///// </summary>
        //private bool IsPaintEvent(Event evt)
        //{
        //    if (evt.alt)
        //    {
        //        return false;
        //    }

        //    if (evt.type != EventType.MouseDown && evt.type != EventType.MouseDrag)
        //    {
        //        return false;
        //    }

        //    return evt.button == 0 || evt.button == 1;
        //}

        ///// <summary>
        ///// 현재 마우스 위치와 브러시 기준으로 칠할 Tilemap과 셀을 찾습니다.
        ///// </summary>
        //private bool TryGetPaintTarget(Event evt, out Tilemap tilemap, out WorldTilemapData tilemapData, out WorldTilemapType tilemapType, out Vector3Int cellPos)
        //{
        //    tilemap = null;
        //    tilemapData = null;
        //    tilemapType = WorldTilemapType.None;
        //    cellPos = default;

        //    if (IsPaintReady() == false)
        //    {
        //        return false;
        //    }

        //    tilemapType = GetTilemapType(currBrush.TileType);
        //    if (TryGetOrCreateTilemap(tilemapType, out tilemap) == false)
        //    {
        //        return false;
        //    }

        //    if (TryGetOrCreateTilemapData(tilemapType, out tilemapData) == false)
        //    {
        //        return false;
        //    }

        //    Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
        //    Plane tilemapPlane = new Plane(Vector3.forward, tilemap.transform.position);
        //    if (tilemapPlane.Raycast(ray, out float enter) == false)
        //    {
        //        return false;
        //    }

        //    Vector3 worldPos = ray.GetPoint(enter);
        //    cellPos = tilemap.WorldToCell(worldPos);
        //    cellPos.z = 0;

        //    return true;
        //}

        ///// <summary>
        ///// 현재 브러시로 단일 셀을 칠하고 저장 데이터도 즉시 갱신합니다.
        ///// </summary>
        //private bool PaintCell(Tilemap tilemap, WorldTilemapData tilemapData, Vector3Int cellPos)
        //{
        //    if (currBrush.TryGetRandomTile(out TileBase tileBase, out float gravity) == false)
        //    {
        //        return false;
        //    }

        //    tilemap.SetTile(cellPos, tileBase);

        //    WorldTile worldTile = new WorldTile
        //    {
        //        Type = currBrush.TileType,
        //        Pos = new Vector2Int(cellPos.x, cellPos.y),
        //        Gravity = gravity,
        //        TileBase = tileBase
        //    };

        //    tilemapData.SetTile(worldTile);
        //    EditorUtility.SetDirty(tilemapData);
        //    EditorUtility.SetDirty(mapData);

        //    return true;
        //}

        ///// <summary>
        ///// 단일 셀의 Tilemap 타일과 저장 데이터를 제거합니다.
        ///// </summary>
        //private bool EraseCell(Tilemap tilemap, WorldTilemapData tilemapData, Vector3Int cellPos)
        //{
        //    Vector2Int mapPos = new Vector2Int(cellPos.x, cellPos.y);
        //    bool hadTile = tilemap.GetTile(cellPos) != null;
        //    bool removedData = tilemapData.RemoveTile(mapPos);

        //    if (hadTile == false && removedData == false)
        //    {
        //        return false;
        //    }

        //    tilemap.SetTile(cellPos, null);
        //    EditorUtility.SetDirty(tilemapData);
        //    EditorUtility.SetDirty(mapData);

        //    return true;
        //}

        ///// <summary>
        ///// 브러시 타입을 저장 대상 Tilemap 레이어로 변환합니다.
        ///// </summary>
        //private WorldTilemapType GetTilemapType(WorldTileType tileType)
        //{
        //    if ((tileType & WorldTileType.Background) == WorldTileType.Background)
        //    {
        //        return WorldTilemapType.WorldTilemapBackground;
        //    }

        //    return WorldTilemapType.WorldTilemapGround;
        //}

        ///// <summary>
        ///// 현재 로드된 맵에서 Tilemap을 찾고, 없으면 생성합니다.
        ///// </summary>
        //private bool TryGetOrCreateTilemap(WorldTilemapType tilemapType, out Tilemap tilemap)
        //{
        //    tilemap = null;

        //    if (loadedMap == null || tilemapType == WorldTilemapType.None)
        //    {
        //        return false;
        //    }

        //    Tilemap[] tilemaps = loadedMap.GetComponentsInChildren<Tilemap>();
        //    for (int i = 0; i < tilemaps.Length; i++)
        //    {
        //        if (tilemaps[i].name != tilemapType.ToString())
        //        {
        //            continue;
        //        }

        //        tilemap = tilemaps[i];
        //        return true;
        //    }

        //    tilemap = CreateTilemap(tilemapType);
        //    return true;
        //}

        ///// <summary>
        ///// MapData에서 TilemapData를 찾고, 없으면 같은 폴더에 새 에셋으로 생성합니다.
        ///// </summary>
        //private bool TryGetOrCreateTilemapData(WorldTilemapType tilemapType, out WorldTilemapData tilemapData)
        //{
        //    Dictionary<WorldTilemapType, WorldTilemapData> tilemapDatas = mapData.TilemapDatas;
        //    if (tilemapDatas.TryGetValue(tilemapType, out tilemapData) && tilemapData != null)
        //    {
        //        return true;
        //    }

        //    tilemapData = CreateTilemapDataAsset(tilemapType);
        //    if (tilemapData == null)
        //    {
        //        return false;
        //    }

        //    mapData.SetTilemapData(tilemapType, tilemapData);
        //    EditorUtility.SetDirty(mapData);

        //    return true;
        //}

        ///// <summary>
        ///// 현재 Data 에셋 옆에 레이어별 TilemapData 에셋을 생성합니다.
        ///// </summary>
        //private WorldTilemapData CreateTilemapDataAsset(WorldTilemapType tilemapType)
        //{
        //    string mapDataPath = AssetDatabase.GetAssetPath(mapData);
        //    if (string.IsNullOrEmpty(mapDataPath))
        //    {
        //        Debug.LogWarning($"{nameof(mapData)}가 프로젝트 에셋이 아니어서 {nameof(WorldTilemapData)}를 저장할 수 없습니다.", this);
        //        return null;
        //    }

        //    string directoryPath = Path.GetDirectoryName(mapDataPath);
        //    string assetPath = Path.Combine(directoryPath, $"SO_{mapData.name}_{tilemapType}.asset").Replace('\\', '/');
        //    assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

        //    WorldTilemapData tilemapData = ScriptableObject.CreateInstance<WorldTilemapData>();
        //    AssetDatabase.CreateAsset(tilemapData, assetPath);
        //    AssetDatabase.SaveAssets();

        //    return tilemapData;
        //}

        ///// <summary>
        ///// 같은 드래그 입력에서 같은 셀을 반복 처리하지 않도록 확인합니다.
        ///// </summary>
        //private bool IsSamePaintStroke(int button, WorldTilemapType tilemapType, Vector3Int cellPos)
        //{
        //    return lastPaintButton == button && lastPaintTilemapType == tilemapType && lastPaintCellPos == cellPos;
        //}

        ///// <summary>
        ///// 마우스 입력이 끝나면 다음 클릭에서 같은 셀도 다시 처리할 수 있게 초기화합니다.
        ///// </summary>
        //private void ResetPaintStroke()
        //{
        //    if (hasPaintChanges)
        //    {
        //        // 한 스트로크가 끝날 때만 에셋을 저장해 드래그 중 잦은 디스크 쓰기를 피합니다.
        //        AssetDatabase.SaveAssets();
        //        hasPaintChanges = false;
        //    }

        //    lastPaintButton = -1;
        //    lastPaintTilemapType = WorldTilemapType.None;
        //}

        ///// <summary>
        ///// 현재 Tilemap에 배치된 타일을 저장 데이터에 반영합니다.
        ///// </summary>
        //private void SaveTilemap(Tilemap tilemap, WorldTilemapData tilemapData)
        //{
        //    List<WorldTile> tiles = new();
        //    tilemap.CompressBounds();

        //    foreach (Vector3Int cellPos in tilemap.cellBounds.allPositionsWithin)
        //    {
        //        TileBase tileBase = tilemap.GetTile(cellPos);
        //        if (tileBase == null)
        //        {
        //            continue;
        //        }

        //        Vector2Int mapPos = new Vector2Int(cellPos.x, cellPos.y);
        //        tilemapData.TryGetTile(mapPos.x, mapPos.y, out WorldTile worldTile);
        //        worldTile.Pos = mapPos;
        //        worldTile.TileBase = tileBase;

        //        tiles.Add(worldTile);
        //    }

        //    tilemapData.SetTiles(tiles);

        //    EditorUtility.SetDirty(tilemapData);
        //}

        ///// <summary>
        ///// 이전에 생성한 맵이 있으면 제거해 중복 생성을 막습니다.
        ///// </summary>
        //private void ClearLoadedMap()
        //{
        //    if (loadedMap == null)
        //    {
        //        return;
        //    }

        //    DestroyImmediate(loadedMap);
        //    loadedMap = null;
        //}

        ///// <summary>
        ///// 지정한 월드 타일맵 타입에 맞는 Tilemap 자식을 생성합니다.
        ///// </summary>
        //private Tilemap CreateTilemap(WorldTilemapType tilemapType)
        //{
        //    GameObject tilemapObject = new GameObject(tilemapType.ToString());
        //    tilemapObject.transform.SetParent(loadedMap.transform, false);

        //    int layer = LayerMask.NameToLayer(tilemapType.ToString());
        //    if (layer >= 0)
        //    {
        //        tilemapObject.layer = layer;
        //    }

        //    Tilemap tilemap = tilemapObject.AddComponent<Tilemap>();
        //    TilemapRenderer renderer = tilemapObject.AddComponent<TilemapRenderer>();
        //    renderer.sortingOrder = GetSortingOrder(tilemapType);

        //    return tilemap;
        //}

        ///// <summary>
        ///// 저장된 셀 데이터를 Unity Tilemap에 반영합니다.
        ///// </summary>
        //private void DrawTilemap(Tilemap tilemap, WorldTilemapData tilemapData)
        //{
        //    foreach (KeyValuePair<Vector2Int, WorldTile> pair in tilemapData.MapTiles)
        //    {
        //        WorldTile worldTile = pair.Value;
        //        tilemap.SetTile(new Vector3Int(worldTile.Pos.x, worldTile.Pos.y, 0), worldTile.TileBase);
        //    }
        //}

        ///// <summary>
        ///// 레이어별 기본 렌더 순서를 반환합니다.
        ///// </summary>
        //private int GetSortingOrder(WorldTilemapType tilemapType)
        //{
        //    switch (tilemapType)
        //    {
        //        case WorldTilemapType.WorldTilemapBackground:
        //            return -10;

        //        case WorldTilemapType.WorldTilemapUI:
        //            return 10;

        //        default:
        //            return 0;
        //    }
        //}
    }
}
#endif
