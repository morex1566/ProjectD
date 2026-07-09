using System;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 클라이언트 전역 시스템의 초기화 순서를 관리하는 진입점입니다.
    /// </summary>
    public class GameManager : MonoBehaviourSingleton<GameManager>, IDisposable
    {
        private bool isDisposed = false;

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
        /// 씬 오브젝트가 로드된 뒤 씬 의존 매니저들을 초기화합니다.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnAfterSceneLoaded()
        {
            UIManager.Init();
            WorldManager.Init();
            PlayerManager.Init();
        }

        /// <summary>
        /// GameManager 싱글톤 인스턴스를 보장하고 종료 이벤트를 연결합니다.
        /// </summary>
        private static void Init()
        {
            GetInstance();

            Application.quitting -= OnApplicationQuitting;
            Application.quitting += OnApplicationQuitting;
        }

        /// <summary>
        /// 애플리케이션 종료 시점에 모든 매니저 리소스를 정리합니다.
        /// </summary>
        private static void OnApplicationQuitting()
        {
            if (TryGetInstance(out GameManager manager) == false)
            {
                return;
            }

            manager.Dispose();
        }

        /// <summary>
        /// 초기화 역순으로 전역 매니저를 정리합니다.
        /// </summary>
        public void Dispose()
        {
            if (isDisposed == true)
            {
                return;
            }

            isDisposed = true;
            Application.quitting -= OnApplicationQuitting;

            DisposeManager<PlayerManager>();
            DisposeManager<WorldManager>();
            DisposeManager<UIManager>();
            DisposeManager<InputManager>();
            DisposeManager<DOTweenManager>();
            DisposeManager<ResourceManager>();
        }

        protected override void OnDestroy()
        {
            Dispose();
            base.OnDestroy();
        }

        /// <summary>
        /// 이미 존재하는 매니저 인스턴스만 정리합니다.
        /// </summary>
        private static void DisposeManager<T>() where T : MonoBehaviour, IDisposable
        {
            if (MonoBehaviourSingleton<T>.TryGetInstance(out T manager) == false)
            {
                return;
            }

            manager.Dispose();
        }
    }
}
