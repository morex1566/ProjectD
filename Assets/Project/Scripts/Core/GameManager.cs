using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 클라이언트 전역 시스템의 초기화 순서를 관리하는 진입점입니다.
    /// </summary>
    public class GameManager : MonoBehaviourSingleton<GameManager>
    {
        /// <summary>
        /// 씬 로드 전 필요한 전역 시스템과 런타임 리소스를 초기화합니다.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void OnBeforeSplashSceneLoaded()
        {
            Init();
            ResourceManager.Init();
            DOTweenManager.Init();
            InputManager.Init();
        }

        /// <summary>
        /// 씬 오브젝트가 로드된 뒤 씬 의존 UI 매니저를 초기화합니다.
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

        /// <summary>
        /// 초기화 역순으로 전역 매니저를 제거합니다.
        /// </summary>
        protected override void OnDestroy()
        {
            UIManager.Destroy();
            InputManager.Destroy();
            DOTweenManager.Destroy();
            ResourceManager.Destroy();

            base.OnDestroy();
        }
    }
}
