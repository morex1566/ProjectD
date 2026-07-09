using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 씬 오브젝트를 Layer 기준으로 찾는 헬퍼입니다.
    /// </summary>
    public static class GameObjectEx
    {
        /// <summary>
        /// 씬에 존재하는 GameObject 중 지정한 Layer를 가진 첫 번째 오브젝트를 반환합니다.
        /// </summary>
        public static GameObject FindByLayer(int layer)
        {
            GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

            foreach (GameObject obj in allObjects)
            {
                if (obj.layer == layer)
                {
                    return obj;
                }
            }

            return null;
        }

        /// <summary>
        /// 씬에 존재하는 컴포넌트 중 GameObject Layer가 일치하는 첫 번째 컴포넌트를 반환합니다.
        /// </summary>
        public static T FindByLayer<T>(int layer) where T : Component
        {
            T[] components = GameObject.FindObjectsByType<T>(FindObjectsSortMode.None);

            foreach (T component in components)
            {
                if (component.gameObject.layer == layer)
                {
                    return component;
                }
            }

            return null;
        }

        ///<summary>
        /// GameObject에 지정한 컴포넌트가 있으면 반환하고, 없으면 추가해서 반환합니다.
        ///</summary>
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            if (gameObject == null)
            {
                return null;
            }

            // 이미 붙어있는 컴포넌트가 있으면 그대로 사용합니다.
            if (gameObject.TryGetComponent(out T component))
            {
                return component;
            }

            // 없으면 새로 추가해서 반환합니다.
            return gameObject.AddComponent<T>();
        }

        ///<summary>
        /// Component가 붙어있는 GameObject에서 지정한 컴포넌트를 가져오거나 추가합니다.
        ///</summary>
        public static T GetOrAddComponent<T>(this Component component) where T : Component
        {
            if (component == null)
            {
                return null;
            }

            // Component 기준으로도 바로 호출할 수 있게 GameObject 확장 메서드로 넘깁니다.
            return component.gameObject.GetOrAddComponent<T>();
        }

        ///<summary>
        /// 현재 GameObject의 부모, 자기 자신, 자식 계층에서 지정한 컴포넌트를 찾습니다.
        ///</summary>
        public static T GetComponentInHierarchy<T>(this GameObject gameObject, bool includeInactive = true) where T : Component
        {
            if (gameObject == null)
            {
                return null;
            }

            // 먼저 현재 오브젝트와 자식 계층에서 찾습니다.
            T component = gameObject.GetComponentInChildren<T>(includeInactive);

            if (component != null)
            {
                return component;
            }

            // 자식 계층에 없으면 부모 계층에서 찾습니다.
            return gameObject.GetComponentInParent<T>(includeInactive);
        }

        ///<summary>
        /// 현재 Component의 부모, 자기 자신, 자식 계층에서 지정한 컴포넌트를 찾습니다.
        ///</summary>
        public static T GetComponentInHierarchy<T>(this Component component, bool includeInactive = true) where T : Component
        {
            if (component == null)
            {
                return null;
            }

            // Component가 붙어있는 GameObject 기준으로 계층 검색을 실행합니다.
            return component.gameObject.GetComponentInHierarchy<T>(includeInactive);
        }
    }
}
