using DG.DOTweenEditor;
using DG.Tweening;
using TRPG.Runtime;
using UnityEditor;
using UnityEngine;

namespace TRPG.Editor
{
    [CustomEditor(typeof(DOTweenAnimationGroup))]
    public class DOTweenAnimationGroupEditor : UnityEditor.Editor
    {
        private static bool isPreviewing;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUILayout.Space(10f);

            DOTweenAnimationGroup group = (DOTweenAnimationGroup)target;

            if (GUILayout.Button("Refresh Animations"))
            {
                Undo.RecordObject(group, "Refresh DOTween Animations");
                group.Refresh();
                EditorUtility.SetDirty(group);
            }

            GUILayout.Space(5f);

            if (GUILayout.Button("Play"))
            {
                Play(group);
            }

            if (GUILayout.Button("Stop"))
            {
                Stop(group);
            }
        }

        private static void Play(DOTweenAnimationGroup group)
        {
            if (Application.isPlaying)
            {
                group.Play();
                return;
            }

            Stop(group);

            // Edit Mode uses DOTween's editor preview loop because runtime Awake/Start is not called.
            DOTweenEditorPreview.Start();
            RegisterPreviewCleanup();

            bool createdTween = false;
            foreach (DOTweenAnimation animation in group.Animations)
            {
                if (animation == null || !animation.isActive) continue;

                Tween tween = animation.CreateEditorPreview();
                if (tween == null) continue;

                DOTweenEditorPreview.PrepareTweenForPreview(tween);
                createdTween = true;
            }

            isPreviewing = createdTween;
            if (!isPreviewing) DOTweenEditorPreview.Stop(false, true);
        }

        private static void Stop(DOTweenAnimationGroup group)
        {
            if (Application.isPlaying)
            {
                group.Stop();
                return;
            }

            if (!isPreviewing) return;

            // Reset preview targets and kill preview tweens created by this inspector action.
            DOTweenEditorPreview.Stop(true, true);
            isPreviewing = false;

            foreach (DOTweenAnimation animation in group.Animations)
            {
                if (animation == null) continue;

                EditorUtility.SetDirty(animation);
            }
        }

        private static void RegisterPreviewCleanup()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode || !isPreviewing) return;

            // Entering Play Mode must not inherit temporary editor preview tweens.
            DOTweenEditorPreview.Stop(true, true);
            isPreviewing = false;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }
    }
}
