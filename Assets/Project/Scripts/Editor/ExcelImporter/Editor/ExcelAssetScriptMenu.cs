using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;

/// <summary>
/// 선택한 엑셀 파일의 Sheet 목록을 기반으로 ExcelAsset Script 템플릿을 생성합니다.
/// </summary>
public class ExcelAssetScriptMenu
{
	const string ScriptTemplateName = "ExcelAssetScriptTemplete.cs.txt";
	const string FieldTemplete = "\t//public List<EntityType> #FIELDNAME#; // Replace 'EntityType' to an actual Type that is serializable.";

	/// <summary>
	/// 선택한 엑셀 파일에 대응하는 ExcelAsset Script 파일을 생성합니다.
	/// </summary>
	[MenuItem("Assets/Create/ExcelAssetScript", false)]
	static void CreateScript()
	{
		string savePath = EditorUtility.SaveFolderPanel("Save ExcelAssetScript", Application.dataPath, "");
		if(savePath == "") return;

		var selectedAssets = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets);

		string excelPath = AssetDatabase.GetAssetPath(selectedAssets[0]);
		string excelName = Path.GetFileNameWithoutExtension(excelPath);
		List<string> sheetNames = GetSheetNames(excelPath);

		string scriptString = BuildScriptString(excelName, sheetNames);

		string path = Path.ChangeExtension(Path.Combine(savePath, excelName), "cs");
		File.WriteAllText(path, scriptString);

		AssetDatabase.Refresh();
	}

	/// <summary>
	/// 메뉴가 엑셀 파일 하나를 선택했을 때만 활성화되도록 검증합니다.
	/// </summary>
	[MenuItem("Assets/Create/ExcelAssetScript", true)]
	static bool CreateScriptValidation()
	{
		var selectedAssets = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets);
		if(selectedAssets.Length != 1) return false;
		var path = AssetDatabase.GetAssetPath(selectedAssets[0]);
		return Path.GetExtension(path) == ".xls" || Path.GetExtension(path) == ".xlsx";
	}

	/// <summary>
	/// 엑셀 파일의 모든 Sheet 이름을 읽습니다.
	/// </summary>
	static List<string> GetSheetNames(string excelPath)
	{
		var sheetNames = new List<string>();
		using(FileStream stream = File.Open(excelPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
		{
			IWorkbook book = null;
			if (Path.GetExtension(excelPath) == ".xls") book = new HSSFWorkbook(stream);
			else book = new XSSFWorkbook(stream);

			for(int i = 0; i < book.NumberOfSheets; i++)
			{
				var sheet = book.GetSheetAt(i);
				sheetNames.Add(sheet.SheetName);
			}
		}
		return sheetNames;
	}

	/// <summary>
	/// 프로젝트 안에서 ExcelAsset Script 템플릿 파일을 찾아 내용을 읽습니다.
	/// </summary>
	static string GetScriptTempleteString()
	{
		string currentDirectory = Directory.GetCurrentDirectory();
		string[] filePath = Directory.GetFiles(currentDirectory, ScriptTemplateName, SearchOption.AllDirectories);
		if(filePath.Length == 0) throw new Exception("Script template not found.");

		string templateString = File.ReadAllText(filePath[0]);
		return templateString;
	}

	/// <summary>
	/// 템플릿의 자리표시자를 엑셀 이름과 Sheet 필드 목록으로 치환합니다.
	/// </summary>
	static string BuildScriptString(string excelName, List<string> sheetNames)
	{
		string scriptString = GetScriptTempleteString();

		scriptString = scriptString.Replace("#ASSETSCRIPTNAME#", excelName);

		foreach(string sheetName in sheetNames)
		{
			string fieldString = String.Copy(FieldTemplete);
			fieldString = fieldString.Replace("#FIELDNAME#", sheetName);
			fieldString += "\n#ENTITYFIELDS#";
			scriptString = scriptString.Replace("#ENTITYFIELDS#", fieldString);
		}
		scriptString = scriptString.Replace("#ENTITYFIELDS#\n", "");

		return scriptString;
	}
}
