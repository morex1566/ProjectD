using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace TRPG.Runtime
{
    public class TutorialEvent : Event
    {
        [SerializeField] private AssetReferenceT<MapData> tutorialMeetingMapDataRef;

        [SerializeField] private AssetReferenceT<MapData> tutorialCombatMapDataRef;

        [SerializeField] private AssetReferenceT<CreatureData> cheshireDataRef;

        [SerializeField] private AssetReferenceT<DialogueData> tutorialMeetCheshireRef;

        [SerializeField] private AssetReferenceT<DialogueData> tutorialBeforeCombatStartRef;

        private Dialouge meetDialogue;

        private ConversationUI meetDialogueUI;

        private Dialouge beforeCombatStartDialogue;

        private ConversationUI beforeCombatStartDialogueUI;

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
            dialougeUI.OnCompleted += StartMeetEvt;
            dialougeUI.OnCompleted += panelUI.Close;
            dialougeUI.Play(dialouge);

            await eventCompletionSource.Task;
        }

        private void StartMeetEvt()
        {
            // 이벤트 시작
            dialougeUI.Close();
            panelUI.PlayDitherRevealAsync(() => StartMeetEvtAsync().Forget()).Forget();

            eventCompletionSource?.TrySetResult();
        }

        private async UniTask StartMeetEvtAsync()
        {
            // 맵 로드
            var mapData = ResourceManager.GetResource<MapData>(tutorialMeetingMapDataRef);
            WorldManager.SpawnTiles(mapData);

            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

            // 플레이어랑 NPC 스폰
            var cheshireData = ResourceManager.GetResource<CreatureData>(cheshireDataRef);
            WorldManager.SpawnPlayer(Vector3Int.zero);
            WorldManager.SpawnNPC(cheshireData, new Vector3Int(-2, 0, 0));

            await UniTask.Delay(TimeSpan.FromSeconds(0.8f));

            // 대화 시작
            meetDialogue = DialogueManager.Load(tutorialMeetCheshireRef);
            meetDialogueUI = UIManager.OpenStretch<ConversationUI>(UIManager.RenderSpace.Overlay);
            meetDialogueUI.Play(meetDialogue);
            meetDialogueUI.OnCompleted += StartCombatEvt;
            meetDialogueUI.OnCompleted += meetDialogueUI.Close;
        }

        private void StartCombatEvt()
        {
            StartCombatEvtAsync().Forget();
        }

        private async UniTask StartCombatEvtAsync()
        {
            // 맵 로드
            var mapData = ResourceManager.GetResource<MapData>(tutorialCombatMapDataRef);
            WorldManager.SpawnTiles(mapData);
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

            // 적 스폰
            WorldManager.SpawnMonsters(mapData);
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

            // 대화 시작
            beforeCombatStartDialogue = DialogueManager.Load(tutorialBeforeCombatStartRef);
            beforeCombatStartDialogueUI = UIManager.OpenStretch<ConversationUI>(UIManager.RenderSpace.Overlay);
            beforeCombatStartDialogueUI.Play(beforeCombatStartDialogue);
            beforeCombatStartDialogueUI.OnCompleted += beforeCombatStartDialogueUI.Close;
        }
    }
}
