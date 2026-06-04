using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace TRPG.Runtime
{
    public static class DOTweenAnimationEx
    {
        public static async UniTask PlayAsync(DOTweenAnimation animation)
        {
            Restart(animation);

            await WaitForAnimationsAsync(animation);
        }

        public static void Restart(DOTweenAnimation animation)
        {
            if (animation == null) return;

            animation.CreateTween(true, false);

            if (string.IsNullOrEmpty(animation.id))
            {
                animation.DORestart();
                return;
            }

            animation.DORestartById(animation.id);
        }

        public static async UniTask WaitForAnimationsAsync(params DOTweenAnimation[] animations)
        {
            float duration = GetDuration(animations);
            if (duration <= 0f) return;

            await UniTask.Delay(TimeSpan.FromSeconds(duration));
        }

        public static float GetDuration(params DOTweenAnimation[] animations)
        {
            float duration = 0f;

            foreach (DOTweenAnimation animation in animations)
            {
                if (animation == null) continue;

                duration = Mathf.Max(duration, animation.delay + animation.duration);
            }

            return duration;
        }
    }
}
