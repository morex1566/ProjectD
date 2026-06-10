using DG.Tweening;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TRPG.Runtime
{
    /// <summary>
    /// 클라이언트 전역 시스템의 초기화 순서를 관리하는 진입점입니다.
    /// </summary>
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class GameManager : MonoBehaviourSingleton<GameManager>
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void OnBeforeSplashSceneLoaded()
        {
            Init();
            DOTween.Init();
            InputManager.Init();
            ResourceManager.Init();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnAfterSceneLoaded()
        {
            UIManager.Init();
            WorldManager.Init();
        }

        private static void Init()
        {
            GetInstance();
        }
    }
}
