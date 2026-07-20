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

            if (IsSerializableDictionary(property))
            {
                DrawReadOnlySerializableDictionary(position, property, label);
            }
            else if (IsArrayOrList(property))
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
            if (IsSerializableDictionary(property)) return EditorGUIUtility.singleLineHeight;

            if (IsArrayOrList(property)) return GetArrayHeight(property);

            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        /// <summary>
        /// 문자열이 아닌 배열/리스트 프로퍼티인지 확인합니다.
        /// </summary>
        private static bool IsArrayOrList(SerializedProperty property)
        {
            return property.isArray && property.propertyType != SerializedPropertyType.String;
        }

        /// <summary>
        /// SerializableDictionary인지 확인합니다.
        /// </summary>
        private static bool IsSerializableDictionary(SerializedProperty property)
        {
            return property.FindPropertyRelative("entries") != null;
        }

        /// <summary>
        /// 단일 프로퍼티를 비활성 상태로 그립니다.
        /// </summary>
        private static void DrawReadOnlyProperty(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.PropertyField(position, property, label, true);
            EditorGUI.EndDisabledGroup();
        }

        /// <summary>
        /// 읽기 전용 SerializableDictionary는 Entry 전체 대신 저장 개수만 표시합니다.
        /// </summary>
        private static void DrawReadOnlySerializableDictionary(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty entries = property.FindPropertyRelative("entries");
            EditorGUI.LabelField(position, label, new GUIContent(BuildDictionaryCountText(entries)));
        }

        /// <summary>
        /// SerializableDictionary의 저장 개수 표시 문자열을 만듭니다.
        /// </summary>
        private static string BuildDictionaryCountText(SerializedProperty entries)
        {
            if (entries == null)
            {
                return "Count: 0";
            }

            int nestedCount = CountNestedDictionaryValues(entries);
            if (nestedCount >= 0)
            {
                return $"Count: {entries.arraySize} / Nested: {nestedCount}";
            }

            return $"Count: {entries.arraySize}";
        }

        /// <summary>
        /// Value가 SerializableDictionary인 경우 내부 Entry 개수를 합산합니다.
        /// </summary>
        private static int CountNestedDictionaryValues(SerializedProperty entries)
        {
            int totalCount = 0;
            bool hasNestedDictionary = false;

            for (int i = 0; i < entries.arraySize; i++)
            {
                SerializedProperty entry = entries.GetArrayElementAtIndex(i);
                SerializedProperty value = entry.FindPropertyRelative("Value");
                SerializedProperty nestedEntries = value?.FindPropertyRelative("entries");

                if (nestedEntries == null)
                {
                    continue;
                }

                hasNestedDictionary = true;
                totalCount += nestedEntries.arraySize;
            }

            return hasNestedDictionary ? totalCount : -1;
        }

        /// <summary>
        /// 배열/리스트를 펼침 상태와 원소 값만 보이도록 읽기 전용으로 그립니다.
        /// </summary>
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

                // Unity 기본 리스트 UI를 우회해 Size/AddTarget/Remove/Reorder 편집을 막고 값만 표시합니다.
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.PropertyField(lineRect, element, new GUIContent($"Element {i}"), true);
                EditorGUI.EndDisabledGroup();

                lineRect.y += elementHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// 배열 크기 값을 수정 불가능한 라벨로 표시합니다.
        /// </summary>
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

        /// <summary>
        /// 배열/리스트가 Inspector에서 차지할 전체 높이를 계산합니다.
        /// </summary>
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

        /// <summary>
        /// 현재 위치의 한 줄 높이 Rect를 만듭니다.
        /// </summary>
        private static Rect GetLineRect(Rect position)
        {
            return new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        }
    }
}
