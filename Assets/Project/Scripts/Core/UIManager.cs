using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TRPG.Runtime
{
    public class UIManager : MonoBehaviourSingleton<UIManager>
    {
        public UnityEvent<float, float> OnResolutionChanged = new();

        /// <summary>
        /// UI 매니저 초기화 진입점입니다.
        /// </summary>
        public static void Init()
        {
        }
    }
}
