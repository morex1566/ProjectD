using System;
using TRPG.Runtime;
using UnityEditor;
using UnityEngine;

namespace TRPG.Editor
{
    /// <summary>
    /// RootGameObjectOnlyAttribute가 붙은 컴포넌트를 처음 추가할 때 루트 GameObject인지 검증합니다.
    /// </summary>
    [InitializeOnLoad]
    public static class RootGameObjectOnlyEditorValidator
    {
        static RootGameObjectOnlyEditorValidator()
        {
            ObjectFactory.componentWasAdded += OnComponentWasAdded;
        }

        private static void OnComponentWasAdded(Component component)
        {
            if (!IsInvalid(component)) return;

            // 인스펙터에서 컴포넌트를 처음 붙이는 순간에만 제한을 적용합니다.
            Debug.LogError(GetErrorMessage(component), component);
            Undo.DestroyObjectImmediate(component);
        }

        private static bool IsInvalid(Component component)
        {
            MonoBehaviour monoBehaviour = component as MonoBehaviour;
            if (monoBehaviour == null) return false;

            Type componentType = monoBehaviour.GetType();
            return Attribute.IsDefined(componentType, typeof(RootGameObjectOnlyAttribute), true) && monoBehaviour.transform.parent != null;
        }

        private static string GetErrorMessage(Component component)
        {
            return $"[{nameof(RootGameObjectOnlyAttribute)}] {component.GetType().Name} 컴포넌트는 루트 GameObject에만 붙을 수 있습니다. 대상: {GetHierarchyPath(component.transform)}";
        }

        private static string GetHierarchyPath(Transform transform)
        {
            string path = transform.name;
            Transform current = transform.parent;

            while (current != null)
            {
                path = $"{current.name}/{path}";
                current = current.parent;
            }

            return path;
        }
    }
}
