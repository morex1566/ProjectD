using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TRPG.Runtime
{
    public class TutorialEvent : Event
    {
        [SerializeField] private AssetReferenceT<MapData> tutorialMapDataRef;

        [SerializeField] private AssetReferenceT<CreatureData> cheshireDataRef;

        [SerializeField] private AssetReferenceT<DialogueData> conversastionRef;

        private Dialouge conversation;

        private ConversationUI conversationUI;

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
            dialougeUI.OnCompleted += StartEvent;
            dialougeUI.Play(dialouge);

            await eventCompletionSource.Task;
        }

        private void StartEvent()
        {
            // 이벤트 시작
            dialougeUI.Close();
            panelUI.PlayDitherRevealAsync(() => StartEventInternal().Forget()).Forget();

            eventCompletionSource?.TrySetResult();
        }

        private async UniTask StartEventInternal()
        {
            // 기존 띄워진 UI 제거
            panelUI.Close();

            // 맵 스폰
            var mapData = ResourceManager.GetResource<MapData>(tutorialMapDataRef);
            WorldManager.SpawnTiles(mapData);

            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

            // 크리쳐 스폰
            var cheshireData = ResourceManager.GetResource<CreatureData>(cheshireDataRef);
            WorldManager.SpawnPlayer(Vector3Int.zero);
            WorldManager.SpawnNPC(cheshireData, new Vector3Int(-1, 0, 0));

            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

            // 대화 시작
            conversation = DialogueManager.Load(conversastionRef);
            conversationUI = UIManager.OpenStretch<ConversationUI>(UIManager.RenderSpace.Overlay);
            conversationUI.Play(conversation);
            conversationUI.OnCompleted += conversationUI.Close;
        }
    }
}
