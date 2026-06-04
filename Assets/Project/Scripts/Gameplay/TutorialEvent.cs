using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;

namespace TRPG.Runtime
{
    public class TutorialEvent : Event
    {
        [SerializeField] private AssetReferenceT<MapData> tutorialMapDataRef;

        private Dialouge dialouge;

        private DialougeUI dialougeUI;

        private PanelUI panelUI;

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

        private void OnDestroy()
        {
            UnregisterInput();

            eventCompletionSource?.TrySetResult();
        }

        private void OnPlayDialouge(InputAction.CallbackContext context)
        {
            if (dialougeUI == null || dialouge == null) return;

            // 현재 클릭으로 더 이상 출력할 문장이 없다고 확인.
            // 다이얼로그의 이벤트로 넘어감.
            if (!dialougeUI.Play(dialouge))
            {
                UnregisterInput();
                dialougeUI.Close();
                panelUI.PlayDitherRevealAsync(() =>
                {
                    panelUI.Close();
                    RequestSpawn().Forget();
                }).Forget();

                eventCompletionSource?.TrySetResult();
            }
        }

        private void RegisterInput()
        {
            InputManager.InputMappingContext.UI.Submit.performed -= OnPlayDialouge;
            InputManager.InputMappingContext.UI.Submit.performed += OnPlayDialouge;
        }

        private void UnregisterInput()
        {
            InputManager.InputMappingContext.UI.Submit.performed -= OnPlayDialouge;
        }

        private async UniTask RequestSpawn()
        {
            var mapData = ResourceManager.GetResource<MapData>(tutorialMapDataRef);

            // 맵 스폰
            WorldManager.SpawnTiles(mapData);

            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

            // 플레이어 스폰
            WorldManager.SpawnPlayer(Vector3Int.zero);
        }
    }
}
