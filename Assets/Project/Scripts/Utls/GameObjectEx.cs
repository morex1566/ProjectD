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
    }
}
