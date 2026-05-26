using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TRPG.Runtime
{
    /// <summary>
    /// 빌드에 포함된 씬의 이름과 에셋 경로를 직렬화해 참조합니다.
    /// </summary>
    [System.Serializable]
    public struct SceneReference
    {
        [SerializeField] private string sceneName;
        [SerializeField] private string scenePath;

        /// <summary>
        /// 씬 에셋의 이름입니다.
        /// </summary>
        public string SceneName => sceneName;

        /// <summary>
        /// 프로젝트 기준 씬 에셋 경로입니다.
        /// </summary>
        public string ScenePath => scenePath;

        /// <summary>
        /// 씬 참조를 씬 이름 문자열로 변환합니다.
        /// </summary>
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
    /// <summary>
    /// SceneReference를 Inspector에서 SceneAsset 필드처럼 편집하게 해주는 드로어입니다.
    /// </summary>
    [CustomPropertyDrawer(typeof(SceneReference))]
    public class SceneReferenceDrawer : PropertyDrawer
    {
        /// <summary>
        /// SceneAsset 선택값을 씬 이름과 에셋 경로 문자열에 동기화합니다.
        /// </summary>
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
