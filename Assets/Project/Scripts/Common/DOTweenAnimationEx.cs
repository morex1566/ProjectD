using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// DOTweenAnimation을 UniTask와 수동 재생 흐름에서 쓰기 위한 헬퍼입니다.
    /// </summary>
    public static class DOTweenAnimationEx
    {
        /// <summary>
        /// DOTweenAnimation을 처음부터 재생하고 전체 재생 시간이 끝날 때까지 대기합니다.
        /// </summary>
        public static async UniTask PlayAsync(DOTweenAnimation animation)
        {
            Restart(animation);

            await WaitForAnimationsAsync(animation);
        }

        /// <summary>
        /// DOTweenAnimation을 역방향으로 재생하고 전체 재생 시간이 끝날 때까지 대기합니다.
        /// </summary>
        public static async UniTask PlayReverseAsync(DOTweenAnimation animation)
        {
            PlayReverse(animation);

            await WaitForAnimationsAsync(animation);
        }

        /// <summary>
        /// isFrom 값을 임시로 뒤집어 역방향 재생 효과를 만듭니다.
        /// </summary>
        public static void PlayReverse(DOTweenAnimation animation)
        {
            if (animation == null) return;

            bool originIsFrom = animation.isFrom;
            animation.isFrom = !originIsFrom;

            try
            {
                Restart(animation);
            }
            finally
            {
                animation.isFrom = originIsFrom;
            }
        }

        /// <summary>
        /// DOTweenAnimation의 Tween을 생성한 뒤 id 유무에 맞춰 재시작합니다.
        /// </summary>
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

        /// <summary>
        /// 전달된 애니메이션들 중 가장 긴 재생 시간만큼 대기합니다.
        /// </summary>
        public static async UniTask WaitForAnimationsAsync(params DOTweenAnimation[] animations)
        {
            float duration = GetDuration(animations);
            if (duration <= 0f) return;

            await UniTask.Delay(TimeSpan.FromSeconds(duration));
        }

        /// <summary>
        /// 애니메이션들의 delay와 duration을 합산한 최대 시간을 반환합니다.
        /// </summary>
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
