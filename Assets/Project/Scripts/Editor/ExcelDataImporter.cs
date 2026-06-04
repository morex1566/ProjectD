using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using TRPG.Runtime;

namespace TRPG.Editor
{
    /// <summary>
    /// Excel 테이블을 읽어 기존 ScriptableObject 데이터 에셋을 생성하거나 갱신합니다.
    /// </summary>
    public static class ExcelDataImporter
    {
        private const string ExcelDirectory = "Assets/Excels";
        private const string AssetRootDirectory = "Assets/Project/Datas";
        private const string AddressableGroupName = "Remote_Core";
        private const string CreatureDataLabel = "CreatureData";
        private const string ScriptableObjectAssetPrefix = "SO_";
        private const string ExcelLineBreakMarker = @"\n";

        [MenuItem("TRPG/Data/Import Excel Tables")]
        public static void ImportExcelTables()
        {
            List<TableData> tables = LoadTables();
            if (tables.Count == 0)
            {
                Debug.LogWarning($"Excel table not found: {GetExcelDirectoryPath()}");
                return;
            }

            CreateScriptableObjectAssets(tables);
        }

        private static List<TableData> LoadTables()
        {
            string excelDirectoryPath = GetExcelDirectoryPath();
            if (!Directory.Exists(excelDirectoryPath)) return new List<TableData>();

            string[] excelPaths = Directory.GetFiles(excelDirectoryPath, "*.xlsx", SearchOption.TopDirectoryOnly)
                .Where(path => !Path.GetFileName(path).StartsWith("~$", StringComparison.Ordinal))
                .ToArray();

            Dictionary<string, TableData> tables = new Dictionary<string, TableData>();
            foreach (string excelPath in excelPaths)
            {
                foreach (TableData table in ReadWorkbook(excelPath))
                {
                    if (tables.ContainsKey(table.ClassName))
                    {
                        Debug.LogError($"Duplicate sheet class name found: {table.ClassName}");
                        continue;
                    }

                    tables.Add(table.ClassName, table);
                }
            }

            return tables.Values.ToList();
        }

        private static List<TableData> ReadWorkbook(string excelPath)
        {
            List<TableData> tables = new List<TableData>();

            using FileStream workbookStream = File.OpenRead(excelPath);
            using ZipArchive archive = new ZipArchive(workbookStream, ZipArchiveMode.Read);
            List<string> sharedStrings = ReadSharedStrings(archive);
            Dictionary<string, string> relationships = ReadWorkbookRelationships(archive);

            ZipArchiveEntry workbookEntry = archive.GetEntry("xl/workbook.xml");
            if (workbookEntry == null) return tables;

            XDocument workbookDocument = LoadXml(workbookEntry);
            XNamespace spreadsheetNs = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
            XNamespace relationshipNs = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";

            foreach (XElement sheetElement in workbookDocument.Descendants(spreadsheetNs + "sheet"))
            {
                string sheetName = sheetElement.Attribute("name")?.Value;
                string relationshipId = sheetElement.Attribute(relationshipNs + "id")?.Value;
                if (string.IsNullOrWhiteSpace(sheetName) || string.IsNullOrWhiteSpace(relationshipId)) continue;
                if (!relationships.TryGetValue(relationshipId, out string worksheetPath)) continue;

                ZipArchiveEntry worksheetEntry = archive.GetEntry(worksheetPath);
                if (worksheetEntry == null) continue;

                TableData table = ReadWorksheet(sheetName, worksheetEntry, sharedStrings);
                if (table.Fields.Count == 0) continue;

                tables.Add(table);
            }

            return tables;
        }

        private static string GetExcelDirectoryPath()
        {
            return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), ExcelDirectory));
        }

        private static TableData ReadWorksheet(string sheetName, ZipArchiveEntry worksheetEntry, List<string> sharedStrings)
        {
            XDocument worksheetDocument = LoadXml(worksheetEntry);
            XNamespace spreadsheetNs = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
            List<List<string>> rows = new List<List<string>>();

            foreach (XElement rowElement in worksheetDocument.Descendants(spreadsheetNs + "row"))
            {
                SortedDictionary<int, string> cells = new SortedDictionary<int, string>();
                foreach (XElement cellElement in rowElement.Elements(spreadsheetNs + "c"))
                {
                    string cellReference = cellElement.Attribute("r")?.Value;
                    int columnIndex = GetColumnIndex(cellReference);
                    if (columnIndex < 0) continue;

                    cells[columnIndex] = ReadCellValue(cellElement, sharedStrings, spreadsheetNs);
                }

                if (cells.Count == 0)
                {
                    rows.Add(new List<string>());
                    continue;
                }

                int maxColumn = cells.Keys.Max();
                List<string> row = Enumerable.Repeat(string.Empty, maxColumn + 1).ToList();
                foreach (KeyValuePair<int, string> cell in cells)
                {
                    row[cell.Key] = cell.Value;
                }

                rows.Add(row);
            }

            List<string> headers = rows.FirstOrDefault(row => row.Any(value => !string.IsNullOrWhiteSpace(value))) ?? new List<string>();
            List<FieldData> fields = CreateFields(headers);
            List<Dictionary<string, string>> records = CreateRecords(fields, rows.Skip(1));

            return new TableData(sheetName, GetClassName(sheetName), fields, records);
        }

        private static void CreateScriptableObjectAssets(List<TableData> tables)
        {
            EnsureAssetFolder(AssetRootDirectory);

            int createdOrUpdatedCount = 0;
            foreach (TableData table in tables)
            {
                Type schemaType = FindRuntimeType(table.ClassName);
                if (schemaType == null)
                {
                    Debug.LogError($"Schema type not found: TRPG.Runtime.{table.ClassName}");
                    continue;
                }

                string tableAssetDirectory = $"{AssetRootDirectory}/{table.ClassName}";
                EnsureAssetFolder(tableAssetDirectory);

                for (int i = 0; i < table.Records.Count; i++)
                {
                    Dictionary<string, string> record = table.Records[i];
                    string recordId = GetAssetName(table, record);
                    if (string.IsNullOrWhiteSpace(recordId))
                    {
                        Debug.LogWarning($"Excel row skipped because first column id is empty. Sheet: {table.SheetName}, Row: {i + 2}");
                        continue;
                    }

                    string assetName = GetScriptableObjectAssetName(recordId);
                    string assetPath = $"{tableAssetDirectory}/{assetName}.asset";
                    string legacyAssetPath = $"{tableAssetDirectory}/{recordId}.asset";

                    if (!File.Exists(assetPath) && File.Exists(legacyAssetPath))
                    {
                        string moveError = AssetDatabase.MoveAsset(legacyAssetPath, assetPath);
                        if (!string.IsNullOrWhiteSpace(moveError))
                        {
                            Debug.LogWarning($"Legacy excel asset rename failed. From: {legacyAssetPath}, To: {assetPath}, Error: {moveError}");
                        }
                    }

                    ScriptableObject asset = AssetDatabase.LoadAssetAtPath(assetPath, schemaType) as ScriptableObject;
                    if (asset == null)
                    {
                        asset = ScriptableObject.CreateInstance(schemaType);
                        AssetDatabase.CreateAsset(asset, assetPath);
                    }

                    ApplyRecordToAsset(asset, table.Fields, record);
                    RegisterCreatureDataAddressable(asset, assetPath, recordId);
                    EditorUtility.SetDirty(asset);
                    createdOrUpdatedCount++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Excel ScriptableObject import completed. Assets created or updated: {createdOrUpdatedCount}");
        }

        private static void RegisterCreatureDataAddressable(ScriptableObject asset, string assetPath, string assetName)
        {
            if (asset is not CreatureData) return;

            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogWarning("Addressable settings not found. CreatureData asset was created but not registered.");
                return;
            }

            AddressableAssetGroup group = settings.FindGroup(AddressableGroupName) ?? settings.DefaultGroup;
            if (group == null)
            {
                Debug.LogWarning("Addressable group not found. CreatureData asset was created but not registered.");
                return;
            }

            settings.AddLabel(CreatureDataLabel);

            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group);
            entry.address = assetName;
            entry.SetLabel(CreatureDataLabel, true, true);

            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
        }

        private static void ApplyRecordToAsset(ScriptableObject asset, List<FieldData> fields, Dictionary<string, string> record)
        {
            Type assetType = asset.GetType();
            foreach (FieldData field in fields)
            {
                FieldInfo fieldInfo = assetType.GetField(field.FieldName, BindingFlags.Instance | BindingFlags.Public);
                if (fieldInfo == null) continue;

                record.TryGetValue(field.FieldName, out string rawValue);
                if (IsDefaultValueMarker(rawValue))
                {
                    fieldInfo.SetValue(asset, GetDefaultValue(fieldInfo.FieldType));
                    continue;
                }

                object value = ConvertValue(rawValue, fieldInfo.FieldType);
                fieldInfo.SetValue(asset, value);
            }

            ApplyDerivedReferences(asset, record);
        }

        private static void ApplyDerivedReferences(ScriptableObject asset, Dictionary<string, string> record)
        {
            if (asset is not CreatureData creatureData) return;

            creatureData.RefreshDerivedData();
            ApplyMoveRangeData(creatureData, record);
            if (!TryGetRecordValue(record, "PfId", out string pfId)) return;
            if (string.IsNullOrWhiteSpace(pfId)) return;

            GameObject prefab = LoadPrefabByFileName(pfId);
            if (prefab == null)
            {
                Debug.LogWarning($"Creature prefab not found. CreatureData: {creatureData.name}, PfId: {pfId}");
                return;
            }

            creatureData.creaturePf = prefab;
        }

        private static void ApplyMoveRangeData(CreatureData creatureData, Dictionary<string, string> record)
        {
            if (!TryGetRecordValue(record, "MoveRangeData", out string moveRangeName)) return;
            if (string.IsNullOrWhiteSpace(moveRangeName)) return;

            if (CreatureData.TryGetMoveRangeData(moveRangeName, out MoveRangeData moveRangeData))
            {
                creatureData.MoveRangeData = moveRangeData;
                return;
            }

            Debug.LogWarning($"MoveRangeData not found. CreatureData: {creatureData.name}, MoveRange: {moveRangeName}");
        }

        private static object ConvertValue(string rawValue, Type targetType)
        {
            rawValue ??= string.Empty;

            if (targetType == typeof(string)) return ConvertStringValue(rawValue);
            if (targetType == typeof(int))
            {
                return int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) ? value : 0;
            }

            if (targetType == typeof(float))
            {
                return float.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out float value) ? value : 0f;
            }

            if (targetType == typeof(double))
            {
                return double.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out double value) ? value : 0d;
            }

            if (targetType == typeof(bool))
            {
                return bool.TryParse(rawValue, out bool value) && value;
            }

            if (targetType.IsEnum)
            {
                return Enum.TryParse(targetType, rawValue, true, out object value) ? value : Activator.CreateInstance(targetType);
            }

            if (targetType == typeof(MoveRangeData))
            {
                return CreatureData.TryGetMoveRangeData(rawValue, out MoveRangeData moveRangeData) ? moveRangeData : default(MoveRangeData);
            }

            if (targetType == typeof(GameObject))
            {
                return LoadPrefabByFileName(rawValue);
            }

            return null;
        }

        private static string ConvertStringValue(string rawValue)
        {
            rawValue ??= string.Empty;

            // Excel 셀의 수동 줄바꿈은 대화 페이지 전환 마커인 리터럴 "\n"으로 보관합니다.
            return rawValue
                .Replace("\r\n", ExcelLineBreakMarker)
                .Replace("\r", ExcelLineBreakMarker)
                .Replace("\n", ExcelLineBreakMarker);
        }

        private static GameObject LoadPrefabByFileName(string prefabId)
        {
            if (string.IsNullOrWhiteSpace(prefabId)) return null;

            prefabId = Path.GetFileNameWithoutExtension(prefabId.Trim());
            string[] guids = AssetDatabase.FindAssets($"{prefabId} t:Prefab", new[] { "Assets/Project/Prefabs" });
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.Equals(Path.GetFileNameWithoutExtension(assetPath), prefabId, StringComparison.OrdinalIgnoreCase)) continue;

                return AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            }

            return null;
        }

        private static bool TryGetRecordValue(Dictionary<string, string> record, string fieldName, out string value)
        {
            if (record.TryGetValue(fieldName, out value) && !IsDefaultValueMarker(value)) return true;

            value = string.Empty;
            return false;
        }

        private static List<FieldData> CreateFields(List<string> headers)
        {
            List<FieldData> fields = new List<FieldData>();
            for (int column = 0; column < headers.Count; column++)
            {
                string header = headers[column];
                if (string.IsNullOrWhiteSpace(header)) continue;

                string fieldName = NormalizeFieldName(ToPascalCase(header));
                fields.Add(new FieldData(fieldName, column));
            }

            return fields;
        }

        private static List<Dictionary<string, string>> CreateRecords(List<FieldData> fields, IEnumerable<List<string>> dataRows)
        {
            List<Dictionary<string, string>> records = new List<Dictionary<string, string>>();
            foreach (List<string> row in dataRows)
            {
                if (row.All(value => string.IsNullOrWhiteSpace(value))) continue;
                if (row.Any(IsIgnoredRowMarker)) continue;

                Dictionary<string, string> record = new Dictionary<string, string>();
                foreach (FieldData field in fields)
                {
                    record[field.FieldName] = field.ColumnIndex < row.Count ? row[field.ColumnIndex] : string.Empty;
                }

                records.Add(record);
            }

            return records;
        }

        private static string ReadCellValue(XElement cellElement, List<string> sharedStrings, XNamespace spreadsheetNs)
        {
            string type = cellElement.Attribute("t")?.Value;
            if (type == "inlineStr")
            {
                return cellElement.Element(spreadsheetNs + "is")?.Element(spreadsheetNs + "t")?.Value ?? string.Empty;
            }

            string rawValue = cellElement.Element(spreadsheetNs + "v")?.Value ?? string.Empty;
            if (type == "s")
            {
                return int.TryParse(rawValue, out int index) && index >= 0 && index < sharedStrings.Count
                    ? sharedStrings[index]
                    : string.Empty;
            }

            if (type == "b")
            {
                return rawValue == "1" ? "true" : "false";
            }

            return rawValue;
        }

        private static List<string> ReadSharedStrings(ZipArchive archive)
        {
            ZipArchiveEntry sharedStringEntry = archive.GetEntry("xl/sharedStrings.xml");
            if (sharedStringEntry == null) return new List<string>();

            XDocument document = LoadXml(sharedStringEntry);
            XNamespace spreadsheetNs = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
            return document.Descendants(spreadsheetNs + "si")
                .Select(item => string.Concat(item.Descendants(spreadsheetNs + "t").Select(text => text.Value)))
                .ToList();
        }

        private static Dictionary<string, string> ReadWorkbookRelationships(ZipArchive archive)
        {
            Dictionary<string, string> relationships = new Dictionary<string, string>();
            ZipArchiveEntry relationshipEntry = archive.GetEntry("xl/_rels/workbook.xml.rels");
            if (relationshipEntry == null) return relationships;

            XDocument document = LoadXml(relationshipEntry);
            XNamespace relationshipNs = "http://schemas.openxmlformats.org/package/2006/relationships";
            foreach (XElement relationshipElement in document.Descendants(relationshipNs + "Relationship"))
            {
                string id = relationshipElement.Attribute("Id")?.Value;
                string target = relationshipElement.Attribute("Target")?.Value;
                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(target)) continue;

                relationships[id] = target.StartsWith("/", StringComparison.Ordinal)
                    ? target.TrimStart('/')
                    : $"xl/{target}";
            }

            return relationships;
        }

        private static XDocument LoadXml(ZipArchiveEntry entry)
        {
            using Stream stream = entry.Open();
            return XDocument.Load(stream);
        }

        private static Type FindRuntimeType(string className)
        {
            string fullName = $"TRPG.Runtime.{className}";
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(fullName);
                if (type != null) return type;
            }

            return null;
        }

        private static string GetClassName(string sheetName)
        {
            return ToPascalCase(sheetName);
        }

        private static string GetAssetName(TableData table, Dictionary<string, string> record)
        {
            FieldData firstField = table.Fields.OrderBy(field => field.ColumnIndex).FirstOrDefault();
            string id = firstField != null && record.TryGetValue(firstField.FieldName, out string idValue)
                ? idValue
                : string.Empty;

            if (IsDefaultValueMarker(id)) return string.Empty;

            return SanitizeFileName(id.Trim());
        }

        private static string GetScriptableObjectAssetName(string recordId)
        {
            // Excel ID는 런타임 식별자로 유지하고, Unity 에셋 파일명에만 SO_ 접두사를 붙입니다.
            return recordId.StartsWith(ScriptableObjectAssetPrefix, StringComparison.OrdinalIgnoreCase)
                ? recordId
                : $"{ScriptableObjectAssetPrefix}{recordId}";
        }

        private static int GetColumnIndex(string cellReference)
        {
            if (string.IsNullOrWhiteSpace(cellReference)) return -1;

            int index = 0;
            bool hasColumn = false;
            foreach (char character in cellReference)
            {
                if (!char.IsLetter(character)) break;

                hasColumn = true;
                index *= 26;
                index += char.ToUpperInvariant(character) - 'A' + 1;
            }

            return hasColumn ? index - 1 : -1;
        }

        private static string ToPascalCase(string value)
        {
            List<string> tokens = new List<string>();
            StringBuilder tokenBuilder = new StringBuilder();
            foreach (char character in value)
            {
                if (!char.IsLetterOrDigit(character))
                {
                    if (tokenBuilder.Length > 0)
                    {
                        tokens.Add(tokenBuilder.ToString());
                        tokenBuilder.Clear();
                    }

                    continue;
                }

                tokenBuilder.Append(character);
            }

            if (tokenBuilder.Length > 0)
            {
                tokens.Add(tokenBuilder.ToString());
            }

            StringBuilder builder = new StringBuilder();
            foreach (string token in tokens)
            {
                if (token.Equals("ID", StringComparison.OrdinalIgnoreCase))
                {
                    builder.Append("Id");
                    continue;
                }

                if (token.Equals("HP", StringComparison.OrdinalIgnoreCase))
                {
                    builder.Append("Hp");
                    continue;
                }

                builder.Append(char.ToUpperInvariant(token[0]));
                if (token.Length > 1)
                {
                    builder.Append(token.Substring(1));
                }
            }

            if (builder.Length == 0) return "Generated";
            if (char.IsDigit(builder[0])) builder.Insert(0, '_');

            return builder.ToString();
        }

        private static string NormalizeFieldName(string fieldName)
        {
            return fieldName switch
            {
                "PrefabId" => "PfId",
                "PrefabName" => "PrefabAddress",
                "MoveRange" => "MoveRangeData",
                "DefaultSkillID" => "DefaultSkillId",
                _ => fieldName,
            };
        }

        private static string SanitizeFileName(string value)
        {
            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalidChar, '_');
            }

            return value;
        }

        private static object GetDefaultValue(Type targetType)
        {
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
        }

        private static bool IsDefaultValueMarker(string value)
        {
            if (value == null) return false;

            string trimmedValue = value.TrimStart();
            return trimmedValue.StartsWith("#", StringComparison.Ordinal) && !trimmedValue.StartsWith("##", StringComparison.Ordinal);
        }

        private static bool IsIgnoredRowMarker(string value)
        {
            return value != null && value.TrimStart().StartsWith("##", StringComparison.Ordinal);
        }

        private static void EnsureAssetFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath)) return;

            string[] parts = assetPath.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private sealed class TableData
        {
            public TableData(string sheetName, string className, List<FieldData> fields, List<Dictionary<string, string>> records)
            {
                SheetName = sheetName;
                ClassName = className;
                Fields = fields;
                Records = records;
            }

            public string SheetName { get; }

            public string ClassName { get; }

            public List<FieldData> Fields { get; }

            public List<Dictionary<string, string>> Records { get; }
        }

        private sealed class FieldData
        {
            public FieldData(string fieldName, int columnIndex)
            {
                FieldName = fieldName;
                ColumnIndex = columnIndex;
            }

            public string FieldName { get; }

            public int ColumnIndex { get; }
        }
    }
}
