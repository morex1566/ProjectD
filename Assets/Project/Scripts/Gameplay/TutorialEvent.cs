using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TRPG.Runtime
{
    public class TutorialEvent : Event
    {
        private Dialouge dialouge;

        private DialougeUI dialougeUI;

        private PanelUI panelUI;

        private bool isInputRegistered;

        public override async UniTask ExecuteAsync()
        {
            eventCompletionSource = new UniTaskCompletionSource();

            // 너무 빨라서 좀 대기...
            // TODO : 하드코딩
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

            // 디터링 연출용 패널 UI 오픈
            panelUI = UIManager.Open<PanelUI>(UIManager.RenderSpace.Camera, Vector3.zero, siblingIndex: 0);

            // 화면 원래대로?
            UIManager.SetBackgroundColor();

            // 튜토리얼 다이얼로그 UI 오픈
            dialouge = DialogueManager.Load(DialogueManager.settings.TutorialRef);
            dialougeUI = UIManager.Open<DialougeUI>(UIManager.RenderSpace.Camera, Vector3.zero);
            dialougeUI.Play(dialouge);
            RegisterInput();

            await eventCompletionSource.Task;
        }

        private void OnDisable()
        {
            UnregisterInput();
        }

        private void OnDestroy()
        {
            UnregisterInput();

            eventCompletionSource?.TrySetResult();
        }

        private void OnPlayDialouge(InputAction.CallbackContext context)
        {
            if (dialougeUI == null || dialouge == null) return;

            if (!dialougeUI.Play(dialouge))
            {
                // 현재 클릭으로 더 이상 출력할 문장이 없다고 확인되면 이벤트 완료 흐름으로 넘어갑니다.
                UnregisterInput();
                dialougeUI.Close();
                panelUI.PlayDitherRevealAsync(panelUI.Close).Forget();
            }
        }

        private void RegisterInput()
        {
            if (isInputRegistered || InputManager.InputMappingContext == null) return;

            // 대화 진행은 UI 포인터 Click이 아니라 Submit 입력만 소비합니다.
            InputManager.InputMappingContext.UI.Submit.performed += OnPlayDialouge;
            isInputRegistered = true;
        }

        private void UnregisterInput()
        {
            if (!isInputRegistered || InputManager.InputMappingContext == null) return;

            InputManager.InputMappingContext.UI.Submit.performed -= OnPlayDialouge;
            isInputRegistered = false;
        }
    }
}
