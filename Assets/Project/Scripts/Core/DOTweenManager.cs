using DG.Tweening;
using DOTweenSettings = DG.Tweening.Core.DOTweenSettings;

namespace TRPG.Runtime
{
    /// <summary>
    /// DOTween 전역 설정을 프로젝트 리소스 초기화 흐름에 맞춰 적용합니다.
    /// </summary>
    public class DOTweenManager : MonoBehaviourSingleton<DOTweenManager>
    {
        public static DOTweenSettings Settings { get; private set; }

        /// <summary>
        /// Core에 등록된 DOTween 설정을 로드하고 DOTween 정적 옵션에 적용합니다.
        /// </summary>
        public static void Init()
        {
            GetInstance();

            Settings = ResourceManager.GetResource<DOTweenSettings>(UnityConstant.Addressable.Label.Core);
            if (Settings == null)
            {
                DOTween.Init();
                return;
            }

            DOTween.Init(Settings.defaultRecyclable, Settings.useSafeMode, Settings.logBehaviour);
            ApplySettings();
        }

        /// <summary>
        /// DOTweenSettings 에셋 값을 런타임 DOTween 정적 설정으로 복사합니다.
        /// </summary>
        private static void ApplySettings()
        {
            DOTween.safeModeLogBehaviour = Settings.safeModeOptions.logBehaviour;
            DOTween.nestedTweenFailureBehaviour = Settings.safeModeOptions.nestedTweenFailureBehaviour;
            DOTween.timeScale = Settings.timeScale;
            DOTween.unscaledTimeScale = Settings.unscaledTimeScale;
            DOTween.useSmoothDeltaTime = Settings.useSmoothDeltaTime;
            DOTween.maxSmoothUnscaledTime = Settings.maxSmoothUnscaledTime;
            DOTween.showUnityEditorReport = Settings.showUnityEditorReport;
            DOTween.drawGizmos = Settings.drawGizmos;
            DOTween.defaultUpdateType = Settings.defaultUpdateType;
            DOTween.defaultTimeScaleIndependent = Settings.defaultTimeScaleIndependent;
            DOTween.defaultAutoPlay = Settings.defaultAutoPlay;
            DOTween.defaultAutoKill = Settings.defaultAutoKill;
            DOTween.defaultLoopType = Settings.defaultLoopType;
            DOTween.defaultRecyclable = Settings.defaultRecyclable;
            DOTween.defaultEaseType = Settings.defaultEaseType;
            DOTween.defaultEaseOvershootOrAmplitude = Settings.defaultEaseOvershootOrAmplitude;
            DOTween.defaultEasePeriod = Settings.defaultEasePeriod;
            DOTween.debugMode = Settings.debugMode;
            DOTween.debugStoreTargetId = Settings.debugStoreTargetId;
        }
    }
}
