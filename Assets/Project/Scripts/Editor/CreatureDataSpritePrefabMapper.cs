using System;
using System.Collections.Generic;
using System.IO;
using TRPG.Runtime;
using UnityEditor;
using UnityEngine;

namespace TRPG.Editor
{
    /// <summary>
    /// CreatureDataSheet의 CreatureData에 스프라이트/CreatureContext 프리팹 참조를 자동 연결합니다.
    /// </summary>
    public class CreatureDataSpritePrefabMapper : AssetPostprocessor
    {
        private const string MenuPath = "Tools/TRPG/Data/MapController CreatureContext Sprite Prefabs";
        private const string CreatureSheetFilter = "t:CreatureSheet";
        private const string PrefabSearchFolder = "Assets/Project/Prefabs/Gameplay";
        private const string FactionSearchFolder = "Assets/Project/Datas/Gameplay";
        private const string PrefabNamePrefix = "PF_";
        private const string CreaturePrefabPath = "Assets/Project/Prefabs/Gameplay/PF_Creature.prefab";

        private static bool isMappingScheduled;



        /// <summary>
        /// 에디터 로드 직후 프리팹 매핑 작업을 한 번 예약합니다.
        /// </summary>
        [InitializeOnLoadMethod]
        private static void ScheduleInitialMapping()
        {
            ScheduleMapping();
        }

        /// <summary>
        /// 메뉴에서 수동으로 모든 CreatureSheet 프리팹 참조를 갱신합니다.
        /// </summary>
        [MenuItem(MenuPath)]
        public static void MapAllCreatureSheetsFromMenu()
        {
            MapAllCreatureSheets(true);
        }

        /// <summary>
        /// 엑셀, 프리팹, 관련 데이터 에셋 변경이 있으면 프리팹 매핑을 예약합니다.
        /// </summary>
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (!ContainsMappingTarget(importedAssets) &&
                !ContainsMappingTarget(deletedAssets) &&
                !ContainsMappingTarget(movedAssets) &&
                !ContainsMappingTarget(movedFromAssetPaths))
            {
                return;
            }

            ScheduleMapping();
        }

        /// <summary>
        /// 같은 임포트 사이클에서 중복 실행되지 않도록 프리팹 매핑을 지연 예약합니다.
        /// </summary>
        private static void ScheduleMapping()
        {
            if (isMappingScheduled) return;

            isMappingScheduled = true;

            // ExcelImporter가 같은 임포트 패스에서 에셋을 갱신하므로 한 프레임 늦게 매핑합니다.
            EditorApplication.delayCall += () =>
            {
                isMappingScheduled = false;
                MapAllCreatureSheets(false);
            };
        }

        /// <summary>
        /// 변경된 에셋 경로 중 프리팹 매핑이 필요한 대상이 있는지 확인합니다.
        /// </summary>
        private static bool ContainsMappingTarget(IEnumerable<string> assetPaths)
        {
            foreach (string assetPath in assetPaths)
            {
                if (string.IsNullOrEmpty(assetPath)) continue;

                string extension = Path.GetExtension(assetPath);

                if (extension == ".xls" || extension == ".xlsx") return true;
                if (extension == ".prefab" && assetPath.StartsWith(PrefabSearchFolder, StringComparison.Ordinal)) return true;
                if (extension == ".asset" && assetPath.StartsWith(FactionSearchFolder, StringComparison.Ordinal)) return true;
                if (assetPath.EndsWith("CreatureSheet.asset", StringComparison.Ordinal)) return true;
            }

            return false;
        }

        /// <summary>
        /// 모든 CreatureDataSheet를 순회하며 CreatureData의 프리팹 참조를 갱신합니다.
        /// </summary>
        private static void MapAllCreatureSheets(bool logResult)
        {
            Dictionary<string, GameObject> prefabByDataId = BuildPrefabLookup();
            GameObject creaturePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CreaturePrefabPath);
            string[] sheetGuids = AssetDatabase.FindAssets(CreatureSheetFilter);

            int mappedSpriteCount = 0;
            int mappedCreatureCount = 0;
            int mappedFactionCount = 0;
            int missingCount = 0;
            int missingFactionCount = 0;
            bool hasChangedSheet = false;

            foreach (string sheetGuid in sheetGuids)
            {
                string sheetPath = AssetDatabase.GUIDToAssetPath(sheetGuid);
                CreatureSheet sheet = AssetDatabase.LoadAssetAtPath<CreatureSheet>(sheetPath);

                if (sheet == null || sheet.Entities == null) continue;

                bool changed = false;

                foreach (CreatureData creatureData in sheet.Entities)
                {
                    if (creatureData == null || string.IsNullOrWhiteSpace(creatureData.DataId)) continue;

                    string prefabLookupKey = GetResourceLookupKey(creatureData.DataId);

                    if (!prefabByDataId.TryGetValue(prefabLookupKey, out GameObject spritePrefab))
                    {
                        missingCount++;
                        continue;
                    }

                    if (creatureData.SpritePf == spritePrefab) continue;

                    creatureData.SpritePf = spritePrefab;
                    changed = true;
                    mappedSpriteCount++;
                }

                foreach (CreatureData creatureData in sheet.Entities)
                {
                    if (creatureData == null || string.IsNullOrWhiteSpace(creatureData.DataId)) continue;
                    if (creaturePrefab == null) continue;
                    if (creatureData.CreaturePf == creaturePrefab) continue;

                    creatureData.CreaturePf = creaturePrefab;
                    changed = true;
                    mappedCreatureCount++;
                }

                if (!changed) continue;

                EditorUtility.SetDirty(sheet);
                hasChangedSheet = true;
            }

            if (hasChangedSheet)
            {
                AssetDatabase.SaveAssets();
            }

            if (logResult)
            {
                Debug.Log($"CreatureContext prefab mapping complete. Sprite: {mappedSpriteCount}, CreatureContext: {mappedCreatureCount}, Faction: {mappedFactionCount}, Missing Sprite: {missingCount}, Missing Faction: {missingFactionCount}");
            }
        }

        /// <summary>
        /// 프리팹 폴더에서 DataId 조회 키와 GameObject 프리팹 매핑을 만듭니다.
        /// </summary>
        private static Dictionary<string, GameObject> BuildPrefabLookup()
        {
            Dictionary<string, GameObject> prefabByDataId = new();
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { PrefabSearchFolder });

            foreach (string prefabGuid in prefabGuids)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
                string prefabName = Path.GetFileNameWithoutExtension(prefabPath);

                if (string.IsNullOrEmpty(prefabName) ||
                    !prefabName.StartsWith(PrefabNamePrefix, StringComparison.Ordinal))
                {
                    continue;
                }

                string dataId = prefabName.Substring(PrefabNamePrefix.Length);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                if (prefab == null) continue;

                // Data/resource keys are lower camel, but Unity asset names can be PascalCase.
                string lookupKey = GetResourceLookupKey(dataId);
                if (string.IsNullOrEmpty(lookupKey) || prefabByDataId.ContainsKey(lookupKey)) continue;

                // 기본 규칙: Creature_Walker_0000000 -> PF_Creature_Walker_0000000.prefab
                prefabByDataId.Add(lookupKey, prefab);
            }

            return prefabByDataId;
        }

        /// <summary>
        /// 리소스 이름을 구분자 없는 소문자 조회 키로 정규화합니다.
        /// </summary>
        private static string GetResourceLookupKey(string resourceName)
        {
            if (string.IsNullOrWhiteSpace(resourceName)) return string.Empty;

            string trimmedResourceName = resourceName.Trim();
            char[] normalizedChars = new char[trimmedResourceName.Length];
            int normalizedLength = 0;

            foreach (char ch in trimmedResourceName)
            {
                if (ch == '_' || ch == '-' || ch == ' ' || ch == '.') continue;

                normalizedChars[normalizedLength] = char.ToLowerInvariant(ch);
                normalizedLength++;
            }

            return new string(normalizedChars, 0, normalizedLength);
        }
    }
}
