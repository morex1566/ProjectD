using UnityEditor;
using UnityEngine;

namespace TRPG.Editor
{
    /// <summary>
    /// WorldMapEditor의 주요 작업 버튼을 Inspector에 표시합니다.
    /// </summary>
    [CustomEditor(typeof(WorldGridEditor))]
    public class WorldGridEditorInspector : UnityEditor.Editor
    {
        //public override void OnInspectorGUI()
        //{
        //    serializedObject.Update();
        //    DrawPropertiesExcluding(serializedObject, "m_Script");
        //    serializedObject.ApplyModifiedProperties();

        //    EditorGUILayout.Space();
        //    DrawActionButtons();
        //}

        ///// <summary>
        ///// 맵 에디터 작업 버튼을 한 줄로 표시합니다.
        ///// </summary>
        //private void DrawActionButtons()
        //{
        //    WorldGridEditor mapEditor = (WorldGridEditor)target;

        //    using (new EditorGUILayout.HorizontalScope())
        //    {
        //        if (GUILayout.Button("Load"))
        //        {
        //            mapEditor.Load();
        //        }

        //        if (GUILayout.Button("Save"))
        //        {
        //            mapEditor.Save();
        //        }

        //        Color prevColor = GUI.backgroundColor;
        //        GUI.backgroundColor = mapEditor.IsEditMode ? Color.green : prevColor;

        //        if (GUILayout.Button("Edit"))
        //        {
        //            Undo.RecordObject(mapEditor, "Toggle World Context Edit Mode");
        //            mapEditor.SetEditMode(mapEditor.IsEditMode == false);
        //        }

        //        GUI.backgroundColor = prevColor;
        //    }
        //}
    }
}
