using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TRPG.Runtime
{
    public class TitleUI : UIBase
    {
        [SerializeField] private DOTweenAnimation[] anims;


        public event Action OnClick;


        private void Reset()
        {
            Bind();
        }

        private void OnValidate()
        {
            Bind();
        }

        private void Bind()
        {
            anims = GetComponentsInChildren<DOTweenAnimation>(true);
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame ||
                Mouse.current.leftButton.wasPressedThisFrame ||
                Mouse.current.rightButton.wasPressedThisFrame ||
                Mouse.current.middleButton.wasPressedThisFrame)
            {
                OnClick?.Invoke();
            }
        }

        private void OnEnable()
        {
            if (!Application.isPlaying) return;

            PlayOpenAsync().Forget();
        }


        /// <summary>
        /// 타이틀 UI가 화면에 나타나는 애니메이션을 재생합니다.
        /// </summary>
        public async UniTask PlayOpenAsync(Action onCompleted = null)
        {
            foreach (var anim in anims)
            {
                PlayOpenAnim(anim);
            }

            await DOTweenAnimationEx.WaitForAnimationsAsync(anims);

            onCompleted?.Invoke();
        }

        /// <summary>
        /// 타이틀 UI가 화면에서 사라지는 애니메이션을 재생합니다.
        /// </summary>
        public async UniTask PlayExitAsync(Action onCompleted = null)
        {
            foreach (var anim in anims)
            {
                PlayExitAnim(anim);
            }

            await DOTweenAnimationEx.WaitForAnimationsAsync(anims);

            onCompleted?.Invoke();
        }

        private void PlayExitAnim(DOTweenAnimation anim)
        {
            if (anim == null) return;

            // 닫기 애니메이션은 현재 위치/알파에서 To 값으로 내려가며 사라져야 하므로 Rewind하지 않습니다.
            // isFrom 변경은 이미 생성된 tween에 반영되지 않아 현재 상태 기준으로 tween을 재생성합니다.
            anim.isFrom = false;
            anim.RecreateTweenAndPlay();
        }

        private void PlayOpenAnim(DOTweenAnimation anim)
        {
            if (anim == null) return;

            // 열기 애니메이션은 From 값에서 현재 프리팹 배치값으로 올라오며 나타납니다.
            anim.isFrom = true;
            anim.RewindThenRecreateTweenAndPlay();
        }

    }
}
