using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TRPG.Runtime
{
    [System.Serializable]
    public struct SceneReference
    {
        [SerializeField] private string sceneName;
        [SerializeField] private string scenePath;

        public string SceneName => sceneName;
        public string ScenePath => scenePath;

        public static implicit operator string(SceneReference sceneReference)
            => sceneReference.sceneName;

#if UNITY_EDITOR
        [SerializeField] private SceneAsset sceneAsset;

        public static string NameOfSceneAsset => nameof(sceneAsset);
        public static string NameOfSceneName => nameof(sceneName);
        public static string NameOfScenePath => nameof(scenePath);
#endif
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SceneReference))]
    public class SceneReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty sceneAsset = property.FindPropertyRelative(SceneReference.NameOfSceneAsset);
            SerializedProperty sceneName = property.FindPropertyRelative(SceneReference.NameOfSceneName);
            SerializedProperty scenePath = property.FindPropertyRelative(SceneReference.NameOfScenePath);

            EditorGUI.BeginChangeCheck();

            SceneAsset newSceneAsset = EditorGUI.ObjectField(
                position,
                label,
                sceneAsset.objectReferenceValue,
                typeof(SceneAsset),
                false) as SceneAsset;

            if (EditorGUI.EndChangeCheck())
            {
                sceneAsset.objectReferenceValue = newSceneAsset;

                if (newSceneAsset != null)
                {
                    sceneName.stringValue = newSceneAsset.name;
                    scenePath.stringValue = AssetDatabase.GetAssetPath(newSceneAsset);
                }
                else
                {
                    sceneName.stringValue = "";
                    scenePath.stringValue = "";
                }
            }

            EditorGUI.EndProperty();
        }
    }
#endif
}
