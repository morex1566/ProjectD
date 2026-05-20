using UnityEditor;
using UnityEngine;
using TRPG.Runtime;

namespace TRPG.Editor
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        /// <summary>
        /// ReadOnlyAttribute가 붙은 프로퍼티를 Inspector에서 비활성 상태로 그립니다.
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }

        /// <summary>
        /// 기본 프로퍼티 드로어와 같은 높이를 반환합니다.
        /// </summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}
