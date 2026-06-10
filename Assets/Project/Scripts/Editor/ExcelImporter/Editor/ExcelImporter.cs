using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using TRPG.Runtime;

public class ExcelImporter : AssetPostprocessor
{
	const string ScriptableObjectAssetPrefix = "SO_";

	class ExcelAssetInfo
	{
		public Type AssetType { get; set; }
		public ExcelAssetAttribute Attribute { get; set; } 
		public string ExcelName
		{
			get
			{
				return string.IsNullOrEmpty(Attribute.ExcelName) ? AssetType.Name : Attribute.ExcelName;
			}
		}
	}

	static List<ExcelAssetInfo> cachedInfos = null; // Clear on compile.

	static void OnPostprocessAllAssets (string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
	{
		bool imported = false;
		foreach(string path in importedAssets)
		{
			if(Path.GetExtension(path) == ".xls" || Path.GetExtension(path) == ".xlsx") 
			{
				if(cachedInfos == null) cachedInfos = FindExcelAssetInfos();

				var excelName = Path.GetFileNameWithoutExtension(path);
				if(excelName.StartsWith("~$")) continue;

				ExcelAssetInfo info = cachedInfos.Find(i => i.ExcelName == excelName);

				if(info == null) continue;

				ImportExcel(path, info);
				imported = true;
			}
		}

		if(imported) 
		{
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
	}

	static List<ExcelAssetInfo> FindExcelAssetInfos()
	{
		var list = new List<ExcelAssetInfo>();
		foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies())
		{
			foreach(var type in assembly.GetTypes())
			{
				var attributes = type.GetCustomAttributes(typeof(ExcelAssetAttribute), false);
				if(attributes.Length == 0) continue;
				var attribute = (ExcelAssetAttribute)attributes[0];
				var info = new ExcelAssetInfo()
				{
					AssetType = type,
					Attribute = attribute
				};
				list.Add(info);
			}
		}
		return list;
	}

	static UnityEngine.Object LoadOrCreateAsset(string assetPath, Type assetType)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(assetPath));

		var asset = AssetDatabase.LoadAssetAtPath(assetPath, assetType);

		if (asset == null)
		{
			string legacyAssetPath = GetLegacyAssetPath(assetPath, assetType);
			asset = AssetDatabase.LoadAssetAtPath(legacyAssetPath, assetType);

			if (asset != null)
			{
				string error = AssetDatabase.RenameAsset(legacyAssetPath, Path.GetFileNameWithoutExtension(assetPath));

				if (!string.IsNullOrEmpty(error))
				{
					Debug.LogError(error);
				}

				asset = AssetDatabase.LoadAssetAtPath(assetPath, assetType);
			}
		}

		if (asset == null)
		{
			asset = ScriptableObject.CreateInstance(assetType);
			AssetDatabase.CreateAsset((ScriptableObject)asset, assetPath);
			asset.hideFlags = HideFlags.NotEditable;
		}

		string assetName = Path.GetFileNameWithoutExtension(assetPath);

		if (asset.name != assetName)
		{
			asset.name = assetName;
			EditorUtility.SetDirty(asset);
		}

		return asset;
	}

	static string GetLegacyAssetPath(string assetPath, Type assetType)
	{
		string directoryPath = Path.GetDirectoryName(assetPath);
		return Path.Combine(directoryPath, assetType.Name + ".asset");
	}

	static IWorkbook LoadBook(string excelPath)
	{
		using(FileStream stream = File.Open(excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
		{
			if (Path.GetExtension(excelPath) == ".xls") return new HSSFWorkbook(stream);
			else return new XSSFWorkbook(stream);
		}
	}

	static ISheet GetSheetByFieldName(IWorkbook book, string fieldName)
	{
		ISheet sheet = book.GetSheet(fieldName);
		if (sheet != null) return sheet;

		return book.GetSheet(ToLowerCamelName(fieldName));
	}

	static List<string> GetFieldNamesFromSheetHeader(ISheet sheet)
	{
		IRow headerRow = sheet.GetRow(0);

		var fieldNames = new List<string>();
		for (int i = 0; i < headerRow.LastCellNum; i++)
		{
			var cell = headerRow.GetCell(i);
			if(cell == null || cell.CellType == CellType.Blank) break;
			fieldNames.Add(cell.StringCellValue);
		}
		return fieldNames;
	}

	static FieldInfo GetSerializableField(Type entityType, string columnName)
	{
		FieldInfo entityField = entityType.GetField(
			columnName,
			BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
		);

		if (entityField != null) return entityField;

		// Excel/data keys use lower camel while Unity fields use PascalCase.
		string unityFieldName = ToPascalCaseName(columnName);
		if (unityFieldName == columnName) return null;

		return entityType.GetField(
			unityFieldName,
			BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
		);
	}

	static string ToPascalCaseName(string value)
	{
		if (string.IsNullOrWhiteSpace(value)) return value;

		string trimmedValue = value.Trim();
		StringBuilder builder = new StringBuilder(trimmedValue.Length);
		bool upperNext = true;
		bool hasSeparator = false;

		foreach (char ch in trimmedValue)
		{
			if (ch == '_' || ch == '-' || ch == ' ' || ch == '.')
			{
				upperNext = true;
				hasSeparator = true;
				continue;
			}

			builder.Append(upperNext ? char.ToUpperInvariant(ch) : ch);
			upperNext = false;
		}

		if (hasSeparator) return builder.ToString();

		return char.ToUpperInvariant(trimmedValue[0]) + trimmedValue.Substring(1);
	}

	static string ToLowerCamelName(string value)
	{
		if (string.IsNullOrWhiteSpace(value)) return value;

		string trimmedValue = value.Trim();
		return char.ToLowerInvariant(trimmedValue[0]) + trimmedValue.Substring(1);
	}

	static object CellToFieldObject(ICell cell, FieldInfo fieldInfo, bool isFormulaEvalute = false)
	{
		var type = isFormulaEvalute ? cell.CachedFormulaResultType : cell.CellType;

		switch(type)
		{
			case CellType.String:
				if (fieldInfo.FieldType.IsEnum) return Enum.Parse(fieldInfo.FieldType, cell.StringCellValue);
				else return cell.StringCellValue;
			case CellType.Boolean:
				return cell.BooleanCellValue;
			case CellType.Numeric:
				return Convert.ChangeType(cell.NumericCellValue, fieldInfo.FieldType);
			case CellType.Formula:
				if(isFormulaEvalute) return null;
				return CellToFieldObject(cell, fieldInfo, true); 
			default:
				if(fieldInfo.FieldType.IsValueType)
				{
					return Activator.CreateInstance(fieldInfo.FieldType);
				}
				return null;
		}
	}

	static object CreateEntityFromRow(IRow row, List<string> columnNames, Type entityType, string sheetName)
	{
		var entity = Activator.CreateInstance(entityType);

		for (int i = 0; i < columnNames.Count; i++)
		{
			FieldInfo entityField = GetSerializableField(entityType, columnNames[i]);
			if (entityField == null) continue;
			if (!entityField.IsPublic && entityField.GetCustomAttributes(typeof(SerializeField), false).Length == 0) continue;

			ICell cell = row.GetCell(i);
			if (cell == null) continue;

			try
			{
				object fieldValue = CellToFieldObject(cell, entityField);
				entityField.SetValue(entity, fieldValue);
			}
			catch
			{
				throw new Exception(string.Format("Invalid excel cell Type at row {0}, column {1}, {2} sheet.", row.RowNum, cell.ColumnIndex, sheetName));
			}
		}
		return entity;
	}

	static object GetEntityListFromSheet(ISheet sheet, Type entityType)
	{
		List<string> excelColumnNames = GetFieldNamesFromSheetHeader(sheet);

		Type listType = typeof(List<>).MakeGenericType(entityType);
		MethodInfo listAddMethod = listType.GetMethod("Add", new Type[]{entityType});
		object list = Activator.CreateInstance(listType);

		// row of index 0 is header
		for (int i = 1; i <= sheet.LastRowNum; i++)
		{
			IRow row = sheet.GetRow(i);
			if(row == null) break;

			ICell entryCell = row.GetCell(0); 
			if(entryCell == null || entryCell.CellType == CellType.Blank) break;

			// skip comment row
			if(entryCell.CellType == CellType.String && entryCell.StringCellValue.StartsWith("#")) continue;

			var entity = CreateEntityFromRow(row, excelColumnNames, entityType, sheet.SheetName);
			listAddMethod.Invoke(list, new object[] { entity });
		}
		return list;
	}

	static void ImportExcel(string excelPath, ExcelAssetInfo info)
	{
		string assetPath = "";
		string assetName = ScriptableObjectAssetPrefix + info.AssetType.Name + ".asset";

		if(string.IsNullOrEmpty(info.Attribute.AssetPath))
		{
			string basePath = Path.GetDirectoryName(excelPath);
			assetPath = Path.Combine(basePath, assetName);
		}else{
			var path = Path.Combine("Assets", info.Attribute.AssetPath);
			assetPath = Path.Combine(path, assetName);
		}
		UnityEngine.Object asset = LoadOrCreateAsset(assetPath, info.AssetType);

		IWorkbook book = LoadBook(excelPath);

		var assetFields = info.AssetType.GetFields();
		int sheetCount = 0;

		foreach (var assetField in assetFields)
		{
			ISheet sheet =  GetSheetByFieldName(book, assetField.Name);
			if(sheet == null) continue;

			Type fieldType = assetField.FieldType;
			if(! fieldType.IsGenericType || (fieldType.GetGenericTypeDefinition() != typeof(List<>))) continue;

			Type[] types = fieldType.GetGenericArguments();
			Type entityType = types[0];

			object entities = GetEntityListFromSheet(sheet, entityType);
			assetField.SetValue(asset, entities);
			sheetCount++;
		}

		if(info.Attribute.LogOnImport)
		{
			Debug.Log(string.Format("Imported {0} sheets form {1}.", sheetCount, excelPath));
		}

		EditorUtility.SetDirty(asset);
	}
}
