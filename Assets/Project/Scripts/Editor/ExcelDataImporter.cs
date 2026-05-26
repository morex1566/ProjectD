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
using UnityEditor.Callbacks;
using UnityEngine;
using TRPG.Runtime;

namespace TRPG.Editor
{
    /// <summary>
    /// Excel 테이블을 읽어 런타임 데이터 스키마와 ScriptableObject 에셋을 생성합니다.
    /// </summary>
    public static class ExcelDataImporter
    {
        private const string ExcelDirectory = "Excels";
        private const string SchemaDirectory = "Assets/Project/Scripts/Data";
        private const string AssetRootDirectory = "Assets/Project/Datas";
        private const string AddressableGroupName = "Remote_Core";
        private const string CreatureDataLabel = "CreatureData";
        private const string PendingAssetImportKey = "TRPG.ExcelDataImporter.PendingAssetImport";

        [MenuItem("TRPG/Data/Import Excel Tables")]
        public static void ImportExcelTables()
        {
            List<TableData> tables = LoadTables();
            if (tables.Count == 0)
            {
                Debug.LogWarning($"Excel table not found: {GetExcelDirectoryPath()}");
                return;
            }

            bool schemaChanged = GenerateSchemaScripts(tables);
            if (schemaChanged)
            {
                // 스키마 타입이 새로 컴파일된 뒤 같은 Excel을 다시 읽어 에셋을 생성합니다.
                SessionState.SetBool(PendingAssetImportKey, true);
                AssetDatabase.Refresh();
                return;
            }

            CreateScriptableObjectAssets(tables);
        }

        [DidReloadScripts]
        private static void ImportPendingAssets()
        {
            if (!SessionState.GetBool(PendingAssetImportKey, false)) return;

            SessionState.EraseBool(PendingAssetImportKey);

            List<TableData> tables = LoadTables();
            if (tables.Count == 0)
            {
                Debug.LogWarning($"Pending excel import skipped. Excel table not found: {GetExcelDirectoryPath()}");
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
            List<FieldData> fields = CreateFields(headers, rows.Skip(1).ToList());
            List<Dictionary<string, string>> records = CreateRecords(fields, rows.Skip(1));

            return new TableData(sheetName, GetClassName(sheetName), fields, records);
        }

        private static bool GenerateSchemaScripts(List<TableData> tables)
        {
            EnsureAssetFolder(SchemaDirectory);

            bool changed = false;
            foreach (TableData table in tables)
            {
                string scriptPath = $"{SchemaDirectory}/{table.ClassName}.cs";
                string content = CreateSchemaScript(table);
                string currentContent = File.Exists(scriptPath) ? File.ReadAllText(scriptPath, Encoding.UTF8) : string.Empty;

                if (NormalizeLineEndings(currentContent) == content) continue;

                File.WriteAllText(scriptPath, content, new UTF8Encoding(false));
                changed = true;
            }

            if (changed)
            {
                Debug.Log("Excel schema scripts generated. Unity recompilation will create ScriptableObject assets afterward.");
            }

            return changed;
        }

        private static string CreateSchemaScript(TableData table)
        {
            string baseClassName = table.ClassName == "MonsterData" ? "CreatureData" : "ScriptableObject";
            string menuName = table.ClassName == "MonsterData"
                ? "Scriptable Objects/Creature/Monster"
                : $"Scriptable Objects/Data/{table.SchemaName}";
            string fileName = $"SO_{table.SchemaName}";

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("using System;");
            builder.AppendLine("using UnityEngine;");
            builder.AppendLine();
            builder.AppendLine("namespace TRPG.Runtime");
            builder.AppendLine("{");
            builder.AppendLine($"    [CreateAssetMenu(fileName = \"{fileName}\", menuName = \"{menuName}\")]");
            builder.AppendLine($"    public class {table.ClassName} : {baseClassName}");
            builder.AppendLine("    {");

            foreach (FieldData field in table.Fields)
            {
                builder.AppendLine($"        public {field.TypeName} {field.FieldName};");
                builder.AppendLine();
            }

            builder.AppendLine("    }");
            builder.AppendLine("}");

            return NormalizeLineEndings(builder.ToString());
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
                    string assetName = GetAssetName(table, record, i);
                    string assetPath = $"{tableAssetDirectory}/{assetName}.asset";

                    ScriptableObject asset = AssetDatabase.LoadAssetAtPath(assetPath, schemaType) as ScriptableObject;
                    if (asset == null)
                    {
                        asset = ScriptableObject.CreateInstance(schemaType);
                        AssetDatabase.CreateAsset(asset, assetPath);
                    }

                    ApplyRecordToAsset(asset, table.Fields, record);
                    RegisterCreatureDataAddressable(asset, assetPath, assetName);
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
                object value = ConvertValue(rawValue, fieldInfo.FieldType);
                fieldInfo.SetValue(asset, value);
            }
        }

        private static object ConvertValue(string rawValue, Type targetType)
        {
            rawValue ??= string.Empty;

            if (targetType == typeof(string)) return rawValue;
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

            return null;
        }

        private static List<FieldData> CreateFields(List<string> headers, List<List<string>> dataRows)
        {
            List<FieldData> fields = new List<FieldData>();
            for (int column = 0; column < headers.Count; column++)
            {
                string header = headers[column];
                if (string.IsNullOrWhiteSpace(header)) continue;

                string fieldName = NormalizeFieldName(ToPascalCase(header));
                string typeName = InferType(fieldName, dataRows.Select(row => column < row.Count ? row[column] : string.Empty));
                fields.Add(new FieldData(header, fieldName, typeName, column));
            }

            return fields;
        }

        private static List<Dictionary<string, string>> CreateRecords(List<FieldData> fields, IEnumerable<List<string>> dataRows)
        {
            List<Dictionary<string, string>> records = new List<Dictionary<string, string>>();
            foreach (List<string> row in dataRows)
            {
                if (row.All(value => string.IsNullOrWhiteSpace(value))) continue;

                Dictionary<string, string> record = new Dictionary<string, string>();
                foreach (FieldData field in fields)
                {
                    record[field.FieldName] = field.ColumnIndex < row.Count ? row[field.ColumnIndex] : string.Empty;
                }

                records.Add(record);
            }

            return records;
        }

        private static string InferType(string fieldName, IEnumerable<string> rawValues)
        {
            string lowerName = fieldName.ToLowerInvariant();
            if (lowerName is "id" or "displayname" or "prefabaddress" or "defaultskillid" or "description") return "string";
            if (lowerName is "hp" or "damage" or "armor") return "float";
            if (lowerName is "moverange" or "level" or "count") return "int";

            List<string> values = rawValues.Where(value => !string.IsNullOrWhiteSpace(value)).ToList();
            if (values.Count == 0) return "string";

            if (values.All(value => bool.TryParse(value, out _))) return "bool";
            if (values.All(value => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))) return "int";
            if (values.All(value => float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out _))) return "float";

            return "string";
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

        private static string GetAssetName(TableData table, Dictionary<string, string> record, int rowIndex)
        {
            string id = record.TryGetValue("Id", out string idValue) ? idValue : string.Empty;
            string assetId = string.IsNullOrWhiteSpace(id)
                ? (rowIndex + 1).ToString(CultureInfo.InvariantCulture)
                : id;

            return SanitizeFileName($"SO_{assetId}");
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
                "PrefabName" => "PrefabAddress",
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

        private static string NormalizeLineEndings(string value)
        {
            return value.Replace("\r\n", "\n").Replace("\n", "\r\n");
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

            public string SchemaName => ClassName.EndsWith("Data", StringComparison.Ordinal)
                ? ClassName.Substring(0, ClassName.Length - "Data".Length)
                : ClassName;

            public List<FieldData> Fields { get; }

            public List<Dictionary<string, string>> Records { get; }
        }

        private sealed class FieldData
        {
            public FieldData(string header, string fieldName, string typeName, int columnIndex)
            {
                Header = header;
                FieldName = fieldName;
                TypeName = typeName;
                ColumnIndex = columnIndex;
            }

            public string Header { get; }

            public string FieldName { get; }

            public string TypeName { get; }

            public int ColumnIndex { get; }
        }
    }
}
