using UnityEditor;
using UnityEngine;
using TRPG.Runtime;

namespace TRPG.Editor
{
    /// <summary>
    /// SerializableDictionary를 Inspector에서 요약 표시하거나 Entry 목록으로 표시합니다.
    /// </summary>
    [CustomPropertyDrawer(typeof(SerializableDictionary<,>), true)]
    public class SerializableDictionaryDrawer : PropertyDrawer
    {
        private const float CountWidth = 150f;
        private const float ToggleWidth = 70f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty entries = property.FindPropertyRelative("entries");
            SerializedProperty showEntries = property.FindPropertyRelative("showEntriesInInspector");

            EditorGUI.BeginProperty(position, label, property);

            Rect lineRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            Rect toggleRect = new Rect(lineRect.xMax - ToggleWidth, lineRect.y, ToggleWidth, lineRect.height);
            Rect countRect = new Rect(toggleRect.x - CountWidth, lineRect.y, CountWidth, lineRect.height);
            Rect labelRect = new Rect(lineRect.x, lineRect.y, Mathf.Max(0f, countRect.x - lineRect.x), lineRect.height);

            bool shouldShowEntries = showEntries == null || showEntries.boolValue;

            if (shouldShowEntries)
            {
                property.isExpanded = EditorGUI.Foldout(labelRect, property.isExpanded, label, true);
            }
            else
            {
                EditorGUI.LabelField(labelRect, label);
            }

            EditorGUI.LabelField(countRect, BuildCountText(entries));

            if (showEntries != null)
            {
                showEntries.boolValue = EditorGUI.ToggleLeft(toggleRect, "Values", showEntries.boolValue);
            }

            if (property.isExpanded == false || shouldShowEntries == false || entries == null)
            {
                EditorGUI.EndProperty();
                return;
            }

            EditorGUI.indentLevel++;

            Rect entriesRect = new Rect(
                position.x,
                lineRect.yMax + EditorGUIUtility.standardVerticalSpacing,
                position.width,
                EditorGUI.GetPropertyHeight(entries, true));

            EditorGUI.PropertyField(entriesRect, entries, true);

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty entries = property.FindPropertyRelative("entries");
            SerializedProperty showEntries = property.FindPropertyRelative("showEntriesInInspector");
            bool shouldShowEntries = showEntries == null || showEntries.boolValue;

            float height = EditorGUIUtility.singleLineHeight;

            if (property.isExpanded == false || shouldShowEntries == false || entries == null)
            {
                return height;
            }

            return height + EditorGUIUtility.standardVerticalSpacing + EditorGUI.GetPropertyHeight(entries, true);
        }

        private static string BuildCountText(SerializedProperty entries)
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
    }
}
