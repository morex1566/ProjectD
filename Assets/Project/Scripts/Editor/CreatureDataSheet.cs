using System;
using System.Collections;
using System.Collections.Generic;
using TRPG.Runtime;
using UnityEngine;

[ExcelAsset(AssetPath = "Project/Datas/Gen", ExcelName = "CreatureSheet")]
public class CreatureDataSheet : ScriptableObject
{
	public List<CreatureData> Entities;
}
