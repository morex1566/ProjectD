using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TRPG.Runtime
{
    public class LoadingUI : UIBase
    {
        [SerializeField] private PanelUI panelUI;

        [SerializeField] private DOTweenAnimation panelFadeInAnim;

        [SerializeField] private DOTweenAnimation panelFadeOutAnim;

        [SerializeField] private DOTweenAnimation loadingFadeInAnim;

        [SerializeField] private DOTweenAnimation loadingFadeOutAnim;

        [SerializeField] public UnityEvent onLoadingCompleted = new();

        [SerializeField] public UnityEvent onLoadingExitAnimCompleted = new();

        [SerializeField] public UnityEvent onStart = new();

        private readonly List<Func<UniTask>> loadingTasks = new();

        protected override void Awake()
        {
            base.Awake();
            Bind();
        }

        private void Reset()
        {
            Bind();
        }

        private void OnValidate()
        {
            Bind();
        }

        public void OnEnable()
        {
            if (!Application.isPlaying) return;
        }

        public void AddLoadingTask(Func<UniTask> task)
        {
            if (task == null) return;

            loadingTasks.Add(task);
        }

        /// <summary>
        /// 자동으로 로딩 애님 + 로딩 작업 + 로딩 종료 애님을 순서대로 재생합니다.
        /// </summary>
        /// <returns></returns>
        public async UniTask LoadAsync()
        {
            try
            {
                await PlayFadeInAsync();

                await LoadAsyncInternal();

                await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
            }
            finally
            {
                await PlayFadeOutAsync();

                onLoadingExitAnimCompleted?.Invoke();

                Close();
            }
        }

        public async UniTask LoadAsyncInternal()
        {
            List<UniTask> tasks = new();

            onStart?.Invoke();

            foreach (Func<UniTask> task in loadingTasks)
            {
                tasks.Add(task.Invoke());
            }

            await UniTask.WhenAll(tasks);

            onLoadingCompleted?.Invoke();
        }

        private async UniTask PlayFadeInAsync()
        {
            DOTweenAnimationEx.Restart(loadingFadeInAnim);

            UniTask panelTask = panelUI != null ? panelUI.PlayFadeInAsync() : DOTweenAnimationEx.PlayAsync(panelFadeInAnim);
            UniTask loadingTask = DOTweenAnimationEx.WaitForAnimationsAsync(loadingFadeInAnim);

            await UniTask.WhenAll(panelTask, loadingTask);
        }

        private async UniTask PlayFadeOutAsync()
        {
            DOTweenAnimationEx.Restart(loadingFadeOutAnim);

            UniTask panelTask = panelUI != null ? panelUI.PlayFadeOutAsync() : DOTweenAnimationEx.PlayAsync(panelFadeOutAnim);
            UniTask loadingTask = DOTweenAnimationEx.WaitForAnimationsAsync(loadingFadeOutAnim);

            await UniTask.WhenAll(panelTask, loadingTask);
        }

        private void Bind()
        {
            if (panelUI == null)
            {
                panelUI = GetComponentInChildren<PanelUI>(true);
            }
        }
    }
}
