using UnityEngine;
using DG.Tweening;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TRPG.Runtime
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class GameManager : MonoBehaviourSingleton<GameManager>
    {
        /// <summary>
        /// 씬 로드 전에 전역 매니저와 입력, 리소스 시스템을 초기화합니다.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnBeforeSceneLoaded()
        {
            Init();

            DOTween.Init();
            InputManager.Init();     
            // NetManager.Init();
            ResourceManager.Init();
        }

        /// <summary>
        /// 씬 로드 후 씬 오브젝트 의존성이 있는 시스템을 초기화합니다.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnAfterSceneLoaded()
        {
            UIManager.Init();
        }

        /// <summary>
        /// GameManager 싱글톤 인스턴스를 보장합니다.
        /// </summary>
        private static void Init()
        {
            GetInstance();
        }
    }
}
