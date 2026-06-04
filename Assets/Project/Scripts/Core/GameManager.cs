using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;


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
        /// <summary>
        /// 씬 로드 전에 전역 매니저와 입력, 리소스 시스템을 초기화합니다.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnBeforeSceneLoaded()
        {
            Init();

            DOTween.Init();
            InputManager.Init();     
        }

        /// <summary>
        /// 씬 로드 후 씬 오브젝트 의존성이 있는 시스템을 초기화합니다.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnAfterSceneLoaded()
        {
            ResourceManager.Init();
            UIManager.Init();
            WorldManager.Init();
            EventManager.Init();
            DialogueManager.Init();

            // 시작 씬이여야만 
            if (SceneManager.GetActiveScene().name == "SCN_Title") EventManager.Play<TitleEvent>().Forget();
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
