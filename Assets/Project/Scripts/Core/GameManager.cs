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
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnBeforeSceneLoaded()
        {
            Init();

            DOTween.Init();
            InputManager.Init();     
            // NetManager.Init();
            ResourceManager.Init();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void OnAfterSceneLoaded()
        {
            UIManager.Init();
            Player.Init();
            CreatureSelector.Init();
        }

        private static void Init()
        {
            GetInstance();
        }
    }
}
