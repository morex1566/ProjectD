using UnityEditor;
using UnityEngine;
using TRPG.Runtime;

namespace TRPG.Editor
{
    /// <summary>
    /// ReadOnlyAttribute가 붙은 필드를 Inspector에서 수정할 수 없게 그립니다.
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        private const float SizeLabelWidth = 40f;

        /// <summary>
        /// ReadOnlyAttribute가 붙은 프로퍼티를 Inspector에서 비활성 상태로 그립니다.
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            if (IsArrayOrList(property))
            {
                DrawReadOnlyArray(position, property, label);
            }
            else
            {
                DrawReadOnlyProperty(position, property, label);
            }

            EditorGUI.EndProperty();
        }

        /// <summary>
        /// 기본 프로퍼티 드로어와 같은 높이를 반환합니다.
        /// </summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (IsArrayOrList(property)) return GetArrayHeight(property);

            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        private static bool IsArrayOrList(SerializedProperty property)
        {
            return property.isArray && property.propertyType != SerializedPropertyType.String;
        }

        private static void DrawReadOnlyProperty(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.PropertyField(position, property, label, true);
            EditorGUI.EndDisabledGroup();
        }

        private static void DrawReadOnlyArray(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect lineRect = GetLineRect(position);
            property.isExpanded = EditorGUI.Foldout(lineRect, property.isExpanded, label, true);

            if (!property.isExpanded) return;

            EditorGUI.indentLevel++;

            lineRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            DrawArraySize(lineRect, property.arraySize);

            lineRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            for (int i = 0; i < property.arraySize; i++)
            {
                SerializedProperty element = property.GetArrayElementAtIndex(i);
                float elementHeight = EditorGUI.GetPropertyHeight(element, true);
                lineRect.height = elementHeight;

                // Unity 기본 리스트 UI를 우회해 Size/Add/Remove/Reorder 편집을 막고 값만 표시합니다.
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.PropertyField(lineRect, element, new GUIContent($"Element {i}"), true);
                EditorGUI.EndDisabledGroup();

                lineRect.y += elementHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            EditorGUI.indentLevel--;
        }

        private static void DrawArraySize(Rect position, int size)
        {
            Rect labelRect = position;
            labelRect.width = SizeLabelWidth;

            Rect valueRect = position;
            valueRect.x += SizeLabelWidth;
            valueRect.width -= SizeLabelWidth;

            EditorGUI.LabelField(labelRect, "Size");
            EditorGUI.LabelField(valueRect, size.ToString());
        }

        private static float GetArrayHeight(SerializedProperty property)
        {
            float height = EditorGUIUtility.singleLineHeight;
            if (!property.isExpanded) return height;

            height += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
            for (int i = 0; i < property.arraySize; i++)
            {
                SerializedProperty element = property.GetArrayElementAtIndex(i);
                height += EditorGUIUtility.standardVerticalSpacing + EditorGUI.GetPropertyHeight(element, true);
            }

            return height;
        }

        private static Rect GetLineRect(Rect position)
        {
            return new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        }
    }
}
