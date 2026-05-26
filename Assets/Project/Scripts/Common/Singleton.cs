using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// MonoBehaviour가 아닌 타입을 지연 생성하는 기본 싱글톤입니다.
    /// </summary>
    public class Singleton<T> where T : class, new()
    {
        /// <summary>
        /// 순수 C# 싱글톤 인스턴스를 생성하거나 기존 인스턴스를 반환합니다.
        /// </summary>
        public static T GetInstance()
        {
            if (instance == null)
            {
                instance = new T();
            }

            return instance;
        }

        protected static T instance = null;
    }

    /// <summary>
    /// 씬 전환 후에도 유지되는 MonoBehaviour 싱글톤 기반 클래스입니다.
    /// </summary>
    public class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static bool isQuitting = false;

        protected static GameObject instanceObj = null;

        protected static T instance = null;

        private static GameObject managerRoot = null;

        private const string ManagerRootName = "[Manager]";

        /// <summary>
        /// 씬에 존재하는 MonoBehaviour 싱글톤을 찾거나 매니저 루트 아래에 생성합니다.
        /// </summary>
        public static T GetInstance()
        {
            if (isQuitting) return null;
            if (instance != null) return instance;

            instance = FindAnyObjectByType<T>();
            if (instance == null)
            {
                instanceObj = new GameObject($"[{typeof(T).Name}]");
                instance = instanceObj.AddComponent<T>();
            }
            else
            {
                instanceObj = instance.gameObject;
                instanceObj.name = $"[{typeof(T).Name}]";
            }

            GameObject root = GetManagerRoot();
            if (instanceObj != root)
            {
                instanceObj.transform.SetParent(root.transform);
            }

            return instance;
        }

        /// <summary>
        /// 애플리케이션 종료 중에는 싱글톤 재생성을 막습니다.
        /// </summary>
        protected virtual void OnApplicationQuit()
        {
            isQuitting = true;
        }

        /// <summary>
        /// 현재 인스턴스가 제거될 때 정적 참조를 정리합니다.
        /// </summary>
        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
                instanceObj = null;
            }
        }

        /// <summary>
        /// 싱글톤 GameObject를 명시적으로 제거하고 참조를 초기화합니다.
        /// </summary>
        public static void Destroy()
        {
            if (instanceObj != null)
            {
                Destroy(instanceObj);

                instanceObj = null;
                instance = null;
            }
        }

        /// <summary>
        /// 모든 매니저 싱글톤이 배치될 DontDestroyOnLoad 루트를 반환합니다.
        /// </summary>
        private static GameObject GetManagerRoot()
        {
            if (managerRoot == null)
            {
                managerRoot = GameObject.Find(ManagerRootName);
            }

            if (managerRoot == null)
            {
                managerRoot = new GameObject(ManagerRootName);
            }

            DontDestroyOnLoad(managerRoot);
            return managerRoot;
        }
    }
}
