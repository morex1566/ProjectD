using System;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 게임플레이 UI 루트와 공통 UI 프리팹 생성을 관리합니다.
    /// </summary>
    public class UIManager : MonoBehaviourSingleton<UIManager>, IDisposable
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

        private Action onResolutionChange;

        private bool isDisposed = false;

        public static Action OnResolutionChange
        {
            get => GetInstance().onResolutionChange;
            set => GetInstance().onResolutionChange = value;
        }

        public static UIManagerSettingsData Settings { get; private set; }

        /// <summary>
        /// UI 매니저 초기화 진입점입니다.
        /// </summary>
        public static void Init()
        {
            UIManager manager = GetInstance();
            manager.isDisposed = false;

            Settings = ResourceManager.GetResource<UIManagerSettingsData>(UnityConstant.Addressable.Label.Core);
        }

        /// <summary>
        /// UI 매니저가 보유한 런타임 콜백과 설정 참조를 정리합니다.
        /// </summary>
        public void Dispose()
        {
            if (isDisposed == true)
            {
                return;
            }

            isDisposed = true;

            onResolutionChange = null;
            Settings = null;
        }

        protected override void OnDestroy()
        {
            Dispose();
            base.OnDestroy();
        }
    }
}
