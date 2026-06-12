using System;
using System.Collections.Generic;
using System.IO;
using TRPG.Runtime;
using UnityEditor;
using UnityEngine;

namespace TRPG.Editor
{
    public class CreatureIdKeyDataGenerator : AssetPostprocessor
    {
        private const string MenuPath = "Tools/TRPG/Data/GenerateMap Creature Id Keys";
        private const string CreatureSheetFilter = "t:CreatureDataSheet";
        private const string OutputFolder = "Assets/Project/Datas/Gen";
        private const string AssetNamePrefix = "SO_IdKey_";

        private static bool isGenerationScheduled;



        [InitializeOnLoadMethod]
        private static void ScheduleInitialGeneration()
        {
            ScheduleGeneration();
        }

        [MenuItem(MenuPath)]
        public static void GenerateFromMenu()
        {
            GenerateAllCreatureIdKeys(true);
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (!ContainsGenerationTarget(importedAssets) &&
                !ContainsGenerationTarget(deletedAssets) &&
                !ContainsGenerationTarget(movedAssets) &&
                !ContainsGenerationTarget(movedFromAssetPaths))
            {
                return;
            }

            ScheduleGeneration();
        }

        private static void ScheduleGeneration()
        {
            if (isGenerationScheduled) return;

            isGenerationScheduled = true;

            // ExcelImporter가 CreatureDataSheet를 먼저 갱신한 뒤 IdKeyData를 만들도록 지연 실행합니다.
            EditorApplication.delayCall += () =>
            {
                isGenerationScheduled = false;
                GenerateAllCreatureIdKeys(false);
            };
        }

        private static bool ContainsGenerationTarget(IEnumerable<string> assetPaths)
        {
            foreach (string assetPath in assetPaths)
            {
                if (string.IsNullOrEmpty(assetPath)) continue;

                string extension = Path.GetExtension(assetPath);

                if (extension == ".xls" || extension == ".xlsx") return true;
                if (assetPath.EndsWith("CreatureDataSheet.asset", StringComparison.Ordinal)) return true;
            }

            return false;
        }

        private static void GenerateAllCreatureIdKeys(bool logResult)
        {
            EnsureOutputFolder();

            int createdCount = 0;
            int updatedCount = 0;
            int skippedCount = 0;
            bool hasChangedAsset = false;

            foreach (CreatureData creatureData in LoadCreatureData())
            {
                if (creatureData == null || string.IsNullOrWhiteSpace(creatureData.DataId))
                {
                    skippedCount++;
                    continue;
                }

                IdKeyData idKeyData = LoadOrCreateIdKeyData(creatureData.DataId, ref createdCount);

                if (!ApplyCreatureData(idKeyData, creatureData))
                {
                    continue;
                }

                EditorUtility.SetDirty(idKeyData);
                updatedCount++;
                hasChangedAsset = true;
            }

            if (hasChangedAsset)
            {
                AssetDatabase.SaveAssets();
            }

            if (logResult)
            {
                Debug.Log($"Creature IdKeyData generation complete. Created: {createdCount}, Updated: {updatedCount}, Skipped: {skippedCount}");
            }
        }

        private static IEnumerable<CreatureData> LoadCreatureData()
        {
            HashSet<string> yieldedDataIds = new();
            string[] sheetGuids = AssetDatabase.FindAssets(CreatureSheetFilter);

            foreach (string sheetGuid in sheetGuids)
            {
                string sheetPath = AssetDatabase.GUIDToAssetPath(sheetGuid);
                CreatureDataSheet sheet = AssetDatabase.LoadAssetAtPath<CreatureDataSheet>(sheetPath);

                if (sheet == null || sheet.Entities == null) continue;

                foreach (CreatureData creatureData in sheet.Entities)
                {
                    if (creatureData == null || string.IsNullOrWhiteSpace(creatureData.DataId))
                    {
                        yield return creatureData;
                        continue;
                    }

                    if (!yieldedDataIds.Add(creatureData.DataId))
                    {
                        Debug.LogError($"Duplicate CreatureData DataId detected while generating IdKeyData: {creatureData.DataId}");
                        continue;
                    }

                    yield return creatureData;
                }
            }
        }

        private static IdKeyData LoadOrCreateIdKeyData(string dataId, ref int createdCount)
        {
            string assetPath = GetAssetPath(dataId);
            IdKeyData idKeyData = AssetDatabase.LoadAssetAtPath<IdKeyData>(assetPath);

            if (idKeyData != null)
            {
                RenameAssetIfNeeded(idKeyData, assetPath);
                return idKeyData;
            }

            foreach (string legacyAssetPath in GetLegacyAssetPaths(dataId))
            {
                idKeyData = AssetDatabase.LoadAssetAtPath<IdKeyData>(legacyAssetPath);

                if (idKeyData != null)
                {
                    RenameAssetIfNeeded(idKeyData, assetPath);
                    return idKeyData;
                }
            }

            idKeyData = ScriptableObject.CreateInstance<IdKeyData>();
            AssetDatabase.CreateAsset(idKeyData, assetPath);
            createdCount++;

            return idKeyData;
        }

        private static bool ApplyCreatureData(IdKeyData idKeyData, CreatureData creatureData)
        {
            bool changed = false;

            // CreatureData의 식별/표시 키 3개를 IdKeyData의 같은 순서 필드로 복사합니다.
            changed |= SetIfDifferent(ref idKeyData.Id, creatureData.DataId);
            changed |= SetIfDifferent(ref idKeyData.NameKey, creatureData.NameKey);
            changed |= SetIfDifferent(ref idKeyData.DescKey, creatureData.DescKey);

            return changed;
        }

        private static bool SetIfDifferent(ref string target, string value)
        {
            if (value == null)
            {
                value = string.Empty;
            }

            if (target == value) return false;

            target = value;
            return true;
        }

        private static string GetAssetPath(string dataId)
        {
            string safeDataId = SanitizeFileName(dataId);
            // Unity 에셋 파일명은 data 키와 달리 구분자 기준 Pascal 형태로 생성합니다.
            return $"{OutputFolder}/{AssetNamePrefix}{ToPascalDelimitedName(safeDataId)}.asset";
        }

        private static IEnumerable<string> GetLegacyAssetPaths(string dataId)
        {
            string safeDataId = SanitizeFileName(dataId);

            yield return $"{OutputFolder}/{AssetNamePrefix}{safeDataId}.asset";
            yield return $"{OutputFolder}/{AssetNamePrefix}{ToUpperFirstLetter(safeDataId)}.asset";
        }

        private static void RenameAssetIfNeeded(IdKeyData idKeyData, string desiredAssetPath)
        {
            string currentAssetPath = AssetDatabase.GetAssetPath(idKeyData);
            string currentAssetName = Path.GetFileNameWithoutExtension(currentAssetPath);
            string desiredAssetName = Path.GetFileNameWithoutExtension(desiredAssetPath);

            if (currentAssetName == desiredAssetName) return;

            string error = AssetDatabase.RenameAsset(currentAssetPath, desiredAssetName);

            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError(error);
            }
        }

        private static string SanitizeFileName(string fileName)
        {
            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(invalidChar, '_');
            }

            return fileName;
        }

        private static string ToUpperFirstLetter(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            return char.ToUpperInvariant(value[0]) + value.Substring(1);
        }

        private static string ToPascalDelimitedName(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            char[] chars = value.ToCharArray();
            bool shouldUpper = true;

            for (int i = 0; i < chars.Length; i++)
            {
                char ch = chars[i];

                if (ch == '_' || ch == '-' || ch == ' ' || ch == '.')
                {
                    shouldUpper = true;
                    continue;
                }

                if (shouldUpper)
                {
                    chars[i] = char.ToUpperInvariant(ch);
                    shouldUpper = false;
                }
            }

            return new string(chars);
        }

        private static void EnsureOutputFolder()
        {
            if (AssetDatabase.IsValidFolder(OutputFolder)) return;

            Directory.CreateDirectory(OutputFolder);
            AssetDatabase.ImportAsset(OutputFolder);
        }
    }
}
