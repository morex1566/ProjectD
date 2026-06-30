using System;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 게임플레이 UI 루트와 공통 UI 프리팹 생성을 관리합니다.
    /// </summary>
    public class UIManager : MonoBehaviourSingleton<UIManager>
    {
        /// <summary>
        /// UI가 렌더링될 공간 기준입니다.
        /// </summary>
        public enum RenderSpace
        {
            Overlay,
            World,
            Camera
        }

        public static Action OnResolutionChange;

        public static UIManagerSettingsData Settings { get; private set; }

        /// <summary>
        /// UI 매니저 초기화 진입점입니다.
        /// </summary>
        public static void Init()
        {
            GetInstance();
            Settings = ResourceManager.GetResource<UIManagerSettingsData>(UnityConstant.Addressable.Label.Core);
        }

        
    }
}
