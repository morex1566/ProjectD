using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace TRPG.Runtime
{
    public class PanelUI : UIBase
    {
        [SerializeField] private Image transitionImage;

        [SerializeField] private DOTweenAnimation fadeInAnimation;

        [SerializeField] private DOTweenAnimation fadeOutAnimation;

        [SerializeField] private Material ditherTransitionMaterial;

        [SerializeField] private float ditherDuration = 1.2f;

        private static readonly int ProgressId = Shader.PropertyToID("_Progress");
        private Material runtimeMaterial;
        private Tween ditherTween;
        private int playVersion;
        private bool isDestroyed;


        private void Reset()
        {
            Bind();
        }

        private void OnValidate()
        {
            Bind();
        }

        protected override void Awake()
        {
            base.Awake();
            Bind();

            ApplyMaterial();
        }

        private void OnDestroy()
        {
            isDestroyed = true;
            playVersion++;
            ditherTween?.Kill();

            if (runtimeMaterial != null)
            {
                Destroy(runtimeMaterial);
            }
        }

        public async UniTask PlayRevealAsync()
        {
            await PlayDitherRevealAsync();
        }

        public async UniTask PlayHideAsync()
        {
            await PlayDitherHideAsync();
        }

        public async UniTask PlayFadeInAsync()
        {
            StopDitherAnimation();
            SetTransitionImageActive(true);
            SetTransitionImageAlpha(0f);
            SetProgress(0f);

            await PlayFadeAnimationAsync(fadeInAnimation, 1f, true);
        }

        public async UniTask PlayFadeOutAsync()
        {
            StopDitherAnimation();
            SetTransitionImageActive(true);
            SetTransitionImageAlpha(1f);
            SetProgress(0f);

            await PlayFadeAnimationAsync(fadeOutAnimation, 0f, false);
        }

        public async UniTask PlayDitherRevealAsync(Action onCompleted = null)
        {
            // 0 = 전체 가림, 1 = 전체 노출
            await PlayDitherAsync(0f, 1f, false);

            onCompleted?.Invoke();
        }

        public async UniTask PlayDitherHideAsync(Action onCompleted = null)
        {
            // 1 = 전체 노출, 0 = 전체 가림
            await PlayDitherAsync(1f, 0f, true);

            onCompleted?.Invoke();
        }

        private async UniTask PlayDitherAsync(float from, float to, bool keepImageActive)
        {
            if (transitionImage == null || ditherTransitionMaterial == null) return;

            int version = ++playVersion;

            ApplyMaterial();
            SetTransitionImageActive(true);
            SetTransitionImageAlpha(1f);
            SetProgress(from);

            ditherTween?.Kill();
            ditherTween = GetActiveMaterial()
                .DOFloat(to, ProgressId, Mathf.Max(0f, ditherDuration))
                .SetEase(Ease.Linear);

            await ditherTween.ToUniTask();
            if (isDestroyed || version != playVersion) return;

            SetProgress(to);
            SetTransitionImageActive(keepImageActive);
            ditherTween = null;
        }

        private void Bind()
        {
            if (transitionImage == null)
            {
                transitionImage = GetComponentInChildren<Image>(true);
            }

            DOTweenAnimation[] animations = GetComponentsInChildren<DOTweenAnimation>(true);
            foreach (DOTweenAnimation animation in animations)
            {
                if (animation == null) continue;

                if (fadeInAnimation == null && animation.id == "panelFadeIn")
                {
                    fadeInAnimation = animation;
                    continue;
                }

                if (fadeOutAnimation == null && animation.id == "panelFadeOut")
                {
                    fadeOutAnimation = animation;
                }
            }
        }

        private void ApplyMaterial()
        {
            if (transitionImage == null || ditherTransitionMaterial == null) return;

            if (Application.isPlaying)
            {
                runtimeMaterial ??= Instantiate(ditherTransitionMaterial);
                transitionImage.material = runtimeMaterial;
                return;
            }

            transitionImage.material = ditherTransitionMaterial;
        }

        private Material GetActiveMaterial()
        {
            return runtimeMaterial != null ? runtimeMaterial : ditherTransitionMaterial;
        }

        private void StopDitherAnimation()
        {
            playVersion++;
            ditherTween?.Kill();
            ditherTween = null;
        }

        private void SetProgress(float progress)
        {
            Material activeMaterial = GetActiveMaterial();
            if (activeMaterial == null) return;

            activeMaterial.SetFloat(ProgressId, progress);
        }

        private void SetTransitionImageActive(bool isActive)
        {
            if (transitionImage == null) return;

            transitionImage.gameObject.SetActive(isActive);
        }

        private void SetTransitionImageAlpha(float alpha)
        {
            if (transitionImage == null) return;

            Color color = transitionImage.color;
            color.a = alpha;
            transitionImage.color = color;
        }

        private async UniTask PlayFadeAnimationAsync(DOTweenAnimation animation, float fallbackAlpha, bool keepImageActive)
        {
            if (transitionImage == null) return;

            if (animation == null)
            {
                await transitionImage.DOFade(fallbackAlpha, 0.5f).ToUniTask();
                SetTransitionImageActive(keepImageActive);
                return;
            }

            await DOTweenAnimationEx.PlayAsync(animation);
            SetTransitionImageActive(keepImageActive);
        }
    }
}
