using System;
using System.Collections.Generic;
using System.IO;
using TRPG.Runtime;
using UnityEditor;
using UnityEngine;

namespace TRPG.Editor
{
    /// <summary>
    /// CreatureDataSheet에서 CreatureData 식별 정보를 읽어 IdKeyData 에셋을 자동 생성합니다.
    /// </summary>
    public class CreatureIdKeyDataGenerator : AssetPostprocessor
    {
        private const string MenuPath = "Tools/TRPG/Data/Generate CreatureContext Id Keys";
        private const string CreatureSheetFilter = "t:CreatureSheet";
        private const string OutputFolder = "Assets/Project/Datas/Gen";
        private const string AssetNamePrefix = "SO_IdKey_";

        private static bool isGenerationScheduled;

        /// <summary>
        /// 에디터 로드 직후 IdKeyData 생성 작업을 한 번 예약합니다.
        /// </summary>
        [InitializeOnLoadMethod]
        private static void ScheduleInitialGeneration()
        {
            ScheduleGeneration();
        }

        /// <summary>
        /// 메뉴에서 수동으로 모든 CreatureContext IdKeyData를 생성합니다.
        /// </summary>
        [MenuItem(MenuPath)]
        public static void GenerateFromMenu()
        {
            GenerateAllCreatureIdKeys(true);
        }

        /// <summary>
        /// 엑셀 또는 CreatureSheet 변경이 있으면 IdKeyData 생성을 예약합니다.
        /// </summary>
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

        /// <summary>
        /// 같은 임포트 사이클에서 중복 실행되지 않도록 IdKeyData 생성을 지연 예약합니다.
        /// </summary>
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

        /// <summary>
        /// 변경된 에셋 경로 중 IdKeyData 재생성이 필요한 대상이 있는지 확인합니다.
        /// </summary>
        private static bool ContainsGenerationTarget(IEnumerable<string> assetPaths)
        {
            foreach (string assetPath in assetPaths)
            {
                if (string.IsNullOrEmpty(assetPath)) continue;

                string extension = Path.GetExtension(assetPath);

                if (extension == ".xls" || extension == ".xlsx") return true;
                if (assetPath.EndsWith("CreatureSheet.asset", StringComparison.Ordinal)) return true;
            }

            return false;
        }

        /// <summary>
        /// 모든 CreatureData를 순회하며 대응하는 IdKeyData를 생성하거나 갱신합니다.
        /// </summary>
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
                Debug.Log($"CreatureContext IdKeyData generation complete. Created: {createdCount}, Updated: {updatedCount}, Skipped: {skippedCount}");
            }
        }

        /// <summary>
        /// 프로젝트의 CreatureSheet 에셋들에서 중복 DataId를 제외하고 CreatureData를 열거합니다.
        /// </summary>
        private static IEnumerable<CreatureData> LoadCreatureData()
        {
            HashSet<string> yieldedDataIds = new();
            string[] sheetGuids = AssetDatabase.FindAssets(CreatureSheetFilter);

            foreach (string sheetGuid in sheetGuids)
            {
                string sheetPath = AssetDatabase.GUIDToAssetPath(sheetGuid);
                CreatureSheet sheet = AssetDatabase.LoadAssetAtPath<CreatureSheet>(sheetPath);

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

        /// <summary>
        /// DataId에 대응하는 IdKeyData 에셋을 찾거나 새로 생성합니다.
        /// </summary>
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

        /// <summary>
        /// CreatureData의 키 필드를 IdKeyData에 복사하고 변경 여부를 반환합니다.
        /// </summary>
        private static bool ApplyCreatureData(IdKeyData idKeyData, CreatureData creatureData)
        {
            bool changed = false;

            // CreatureData의 식별/표시 키 3개를 IdKeyData의 같은 순서 필드로 복사합니다.
            changed |= SetIfDifferent(ref idKeyData.Id, creatureData.DataId);
            changed |= SetIfDifferent(ref idKeyData.NameKey, creatureData.NameKey);
            changed |= SetIfDifferent(ref idKeyData.DescKey, creatureData.DescKey);

            return changed;
        }

        /// <summary>
        /// 문자열 값이 달라졌을 때만 대상 필드를 갱신합니다.
        /// </summary>
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

        /// <summary>
        /// DataId를 현재 명명 규칙에 맞는 IdKeyData 에셋 경로로 변환합니다.
        /// </summary>
        private static string GetAssetPath(string dataId)
        {
            string safeDataId = SanitizeFileName(dataId);
            // Unity 에셋 파일명은 data 키와 달리 구분자 기준 Pascal 형태로 생성합니다.
            return $"{OutputFolder}/{AssetNamePrefix}{ToPascalDelimitedName(safeDataId)}.asset";
        }

        /// <summary>
        /// 이전 명명 규칙으로 생성됐을 수 있는 IdKeyData 에셋 경로들을 반환합니다.
        /// </summary>
        private static IEnumerable<string> GetLegacyAssetPaths(string dataId)
        {
            string safeDataId = SanitizeFileName(dataId);

            yield return $"{OutputFolder}/{AssetNamePrefix}{safeDataId}.asset";
            yield return $"{OutputFolder}/{AssetNamePrefix}{ToUpperFirstLetter(safeDataId)}.asset";
        }

        /// <summary>
        /// 기존 에셋 이름이 현재 규칙과 다르면 AssetDatabase에서 이름을 변경합니다.
        /// </summary>
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

        /// <summary>
        /// 파일명에 사용할 수 없는 문자를 밑줄로 치환합니다.
        /// </summary>
        private static string SanitizeFileName(string fileName)
        {
            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(invalidChar, '_');
            }

            return fileName;
        }

        /// <summary>
        /// 문자열의 첫 글자만 대문자로 변환합니다.
        /// </summary>
        private static string ToUpperFirstLetter(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            return char.ToUpperInvariant(value[0]) + value.Substring(1);
        }

        /// <summary>
        /// 구분자를 유지한 채 각 구간의 첫 글자를 대문자로 변환합니다.
        /// </summary>
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

        /// <summary>
        /// IdKeyData 출력 폴더가 없으면 생성하고 AssetDatabase에 임포트합니다.
        /// </summary>
        private static void EnsureOutputFolder()
        {
            if (AssetDatabase.IsValidFolder(OutputFolder)) return;

            Directory.CreateDirectory(OutputFolder);
            AssetDatabase.ImportAsset(OutputFolder);
        }
    }
}
