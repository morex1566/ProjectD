using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

namespace TRPG.Runtime
{
    public class TitleUI : UIBase
    {
        [SerializeField] private DOTweenAnimation[] anims;

        [SerializeField] private TMP_Text messageText = null;

        public event Action OnClick;

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

        private void Bind()
        {
            anims = GetComponentsInChildren<DOTweenAnimation>(true);
            messageText = GetComponentInChildren<TMP_Text>(true);
        }

        private void Update()
        {
            if (HasStartInput())
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
        /// 타이틀 메시지를 외부 이벤트 문구로 교체합니다.
        /// </summary>
        public void SetMessage(string message)
        {
            if (messageText == null) return;

            messageText.text = message;
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

        private bool HasStartInput()
        {
            return (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) ||
                   HasPointerInput();
        }

        private bool HasPointerInput()
        {
            bool isMousePressed = Mouse.current != null &&
                                  (Mouse.current.leftButton.wasPressedThisFrame ||
                                   Mouse.current.rightButton.wasPressedThisFrame ||
                                   Mouse.current.middleButton.wasPressedThisFrame);
            bool isTouchPressed = Touchscreen.current != null &&
                                  Touchscreen.current.primaryTouch.press.wasPressedThisFrame;

            return isMousePressed || isTouchPressed;
        }

    }
}
