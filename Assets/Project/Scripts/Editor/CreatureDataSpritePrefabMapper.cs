using System;
using System.Collections.Generic;
using System.IO;
using TRPG.Runtime;
using UnityEditor;
using UnityEngine;

namespace TRPG.Editor
{
    public class CreatureDataSpritePrefabMapper : AssetPostprocessor
    {
        private const string MenuPath = "Tools/TRPG/Data/Map Creature Sprite Prefabs";
        private const string CreatureSheetFilter = "t:CreatureDataSheet";
        private const string PrefabSearchFolder = "Assets/Project/Prefabs/Gameplay";
        private const string FactionSearchFolder = "Assets/Project/Datas/Gameplay";
        private const string PrefabNamePrefix = "PF_";
        private const string FactionNamePrefix = "SO_";
        private const string CreaturePrefabPath = "Assets/Project/Prefabs/Gameplay/PF_Creature.prefab";

        private static bool isMappingScheduled;



        [InitializeOnLoadMethod]
        private static void ScheduleInitialMapping()
        {
            ScheduleMapping();
        }

        [MenuItem(MenuPath)]
        public static void MapAllCreatureSheetsFromMenu()
        {
            MapAllCreatureSheets(true);
        }

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

        private static bool ContainsMappingTarget(IEnumerable<string> assetPaths)
        {
            foreach (string assetPath in assetPaths)
            {
                if (string.IsNullOrEmpty(assetPath)) continue;

                string extension = Path.GetExtension(assetPath);

                if (extension == ".xls" || extension == ".xlsx") return true;
                if (extension == ".prefab" && assetPath.StartsWith(PrefabSearchFolder, StringComparison.Ordinal)) return true;
                if (extension == ".asset" && assetPath.StartsWith(FactionSearchFolder, StringComparison.Ordinal)) return true;
                if (assetPath.EndsWith("CreatureDataSheet.asset", StringComparison.Ordinal)) return true;
            }

            return false;
        }

        private static void MapAllCreatureSheets(bool logResult)
        {
            Dictionary<string, GameObject> prefabByDataId = BuildPrefabLookup();
            Dictionary<string, FactionData> factionByName = BuildFactionLookup();
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
                CreatureDataSheet sheet = AssetDatabase.LoadAssetAtPath<CreatureDataSheet>(sheetPath);

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
                    if (creatureData == null || string.IsNullOrWhiteSpace(creatureData.Faction)) continue;

                    if (!factionByName.TryGetValue(GetFactionLookupKey(creatureData.Faction), out FactionData factionData))
                    {
                        missingFactionCount++;
                        continue;
                    }

                    if (creatureData.FactionData == factionData) continue;

                    creatureData.FactionData = factionData;
                    changed = true;
                    mappedFactionCount++;
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
                Debug.Log($"Creature prefab mapping complete. Sprite: {mappedSpriteCount}, Creature: {mappedCreatureCount}, Faction: {mappedFactionCount}, Missing Sprite: {missingCount}, Missing Faction: {missingFactionCount}");
            }
        }

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

        private static Dictionary<string, FactionData> BuildFactionLookup()
        {
            Dictionary<string, FactionData> factionByName = new(StringComparer.OrdinalIgnoreCase);
            string[] factionGuids = AssetDatabase.FindAssets("t:FactionData", new[] { FactionSearchFolder });

            foreach (string factionGuid in factionGuids)
            {
                string factionPath = AssetDatabase.GUIDToAssetPath(factionGuid);
                string factionName = Path.GetFileNameWithoutExtension(factionPath);
                FactionData factionData = AssetDatabase.LoadAssetAtPath<FactionData>(factionPath);

                if (factionData == null) continue;

                // 엑셀에는 Human 또는 SO_Human 중 어느 쪽으로 적어도 같은 SO로 매핑합니다.
                AddFactionLookup(factionByName, factionName, factionData);
                if (factionName.StartsWith(FactionNamePrefix, StringComparison.Ordinal))
                {
                    AddFactionLookup(factionByName, factionName.Substring(FactionNamePrefix.Length), factionData);
                }
            }

            return factionByName;
        }

        private static void AddFactionLookup(Dictionary<string, FactionData> factionByName, string factionName, FactionData factionData)
        {
            string factionKey = GetFactionLookupKey(factionName);
            if (string.IsNullOrEmpty(factionKey) || factionByName.ContainsKey(factionKey)) return;

            factionByName.Add(factionKey, factionData);
        }

        private static string GetFactionLookupKey(string factionName)
        {
            return string.IsNullOrWhiteSpace(factionName) ? string.Empty : factionName.Trim();
        }

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
