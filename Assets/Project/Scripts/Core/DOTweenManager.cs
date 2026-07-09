using System;
using DG.Tweening;
using DOTweenSettings = DG.Tweening.Core.DOTweenSettings;

namespace TRPG.Runtime
{
    /// <summary>
    /// DOTween 전역 설정을 프로젝트 리소스 초기화 흐름에 맞춰 적용합니다.
    /// </summary>
    public class DOTweenManager : MonoBehaviourSingleton<DOTweenManager>, IDisposable
    {
        public static DOTweenSettings Settings { get; private set; }

        private bool isDisposed = false;

        /// <summary>
        /// Core에 등록된 DOTween 설정을 로드하고 DOTween 정적 옵션에 적용합니다.
        /// </summary>
        public static void Init()
        {
            DOTweenManager manager = GetInstance();
            manager.isDisposed = false;

            Settings = ResourceManager.GetResource<DOTweenSettings>(UnityConstant.Addressable.Label.Core);
            DOTween.Init();
        }

        /// <summary>
        /// DOTween 전역 Tween과 정적 상태를 정리합니다.
        /// </summary>
        public void Dispose()
        {
            if (isDisposed == true)
            {
                return;
            }

            isDisposed = true;

            DOTween.KillAll();
            DOTween.Clear(true);
            Settings = null;
        }

        protected override void OnDestroy()
        {
            Dispose();
            base.OnDestroy();
        }
    }
}
