using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 타이틀 씬 진입 시 로딩, 코어 리소스 로드, 타이틀 UI 오픈을 순서대로 처리합니다.
    /// </summary>
    public class TitleEvent : Event
    {
        [SerializeField] private Vector3 titleUIPosition = new(0f, -360f, 0f);

        private TitleUI titleUI;

        private bool isStartingTutorial;

        public override async UniTask ExecuteAsync()
        {
            eventCompletionSource = new UniTaskCompletionSource();
            isStartingTutorial = false;

            LoadingUI loading = UIManager.Open<LoadingUI>(UIManager.RenderSpace.Camera, Vector3.zero);
            if (loading == null)
            {
                eventCompletionSource.TrySetResult();
                return;
            }

            loading.AddLoadingTask(LoadCoreResourceAsync);
            loading.onLoadingCompleted.AddListener(UIManager.SetBackgroundColor);
            loading.onLoadingExitAnimCompleted.AddListener(OpenTitleUI);
            loading.LoadAsync().Forget();

            await eventCompletionSource.Task;
        }

        private void OnDestroy()
        {
            eventCompletionSource?.TrySetResult();
        }

        private async UniTask LoadCoreResourceAsync()
        {
            try
            {
                await ResourceManager.LoadAsync(UnityConstant.Addressable.Label.Core);
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
            }
        }

        private void OpenTitleUI()
        {
            titleUI = UIManager.Open<TitleUI>(UIManager.RenderSpace.Camera, titleUIPosition);
            titleUI.OnClick += OnTitleStart;
        }

        /// <summary>
        /// 타이틀 UI 입력을 받아 튜토리얼 시작 흐름을 이벤트가 이어서 처리합니다.
        /// </summary>
        private void OnTitleStart()
        {
            StartTutorialAsync().Forget();
        }

        /// <summary>
        /// 타이틀 UI를 닫고 튜토리얼 다이얼로그 이벤트를 엽니다.
        /// </summary>
        private async UniTask StartTutorialAsync()
        {
            if (isStartingTutorial) return;
            isStartingTutorial = true;

            try
            {
                // 타이틀 UI 닫기
                titleUI.PlayExitAsync(titleUI.Close).Forget();

                // 튜토리얼 이벤트 시작
                var loadingUI = UIManager.Open<LoadingUI>(UIManager.RenderSpace.Camera, Vector3.zero);
                loadingUI.onLoadingCompleted.AddListener(UIManager.SetBackgroundColorBlack);
                loadingUI.onLoadingExitAnimCompleted.AddListener(OnTriggerTutorialEvent);

                await loadingUI.LoadAsync();
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
            }
            finally
            {
                eventCompletionSource?.TrySetResult();
                Close();
            }
        }

        private void OnTriggerTutorialEvent()
        {
            EventManager.Trigger<TutorialEvent>();
        }
    }
}
