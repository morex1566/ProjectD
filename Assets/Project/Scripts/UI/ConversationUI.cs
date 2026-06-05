using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TRPG.Runtime
{
    public class ConversationUI : UIBase
    {
        [Header(nameof(DialougeUI) + ".Setup")]

        [SerializeField] private Image portrait;

        [SerializeField] private TMP_Text speakerText;

        [SerializeField] private TMP_Text dialogueText;

        [SerializeField] private bool showSpeakerText = false;

        [SerializeField] private bool showDialogueText = true;

        [SerializeField] private float typeInterval = 0.08f;

        private bool isTyping;

        private int typingVersion;

        private CancellationTokenSource typingCancellationTokenSource;

        private Dialouge dialogue;

        public event Action OnCompleted;


        protected override void Awake()
        {
            base.Awake();
        }

        private void OnDestroy()
        {
            CancelTypingTask();

            // UI가 사라질 때 진행 중인 타자기 루프가 이후 UI를 만지지 못하게 무효화합니다.
            typingVersion++;
        }

        private void Update()
        {
            if (Keyboard.current == null || !Keyboard.current.spaceKey.wasPressedThisFrame) return;
            if (dialogue == null) return;

            Play(dialogue);
        }


        public void SetPortrait(Sprite sprite)
        {
            portrait.sprite = sprite;
        }

        /// <summary>
        /// 외부에서 대화 데이터를 넘겨받아 대화 출력을 시작합니다.
        /// 출력 중이면 현재 문장을 즉시 완성하고, 끝난 상태면 다음 index의 문장을 출력합니다.
        /// </summary>
        /// <returns>문장을 출력하거나 출력 중인 문장을 완성했으면 true, 더 이상 출력할 텍스트가 없으면 false를 반환합니다.</returns>
        public bool Play(Dialouge dialogue)
        {
            if (isTyping)
            {
                // 현재 문장을 출력 중이면 다음 문장으로 넘기지 않고, 현재 문장만 즉시 완성합니다.
                CompleteTyping();
                return true;
            }

            this.dialogue = dialogue;
            if (this.dialogue == null || this.dialogue.texts == null || this.dialogue.texts.Count == 0)
            {
                // 출력할 대화 데이터 자체가 없으므로 호출자가 대화 종료 처리를 할 수 있게 알립니다.
                OnCompleted?.Invoke();
                return false;
            }

            if (this.dialogue.IsLastIndex)
            {
                // 이전 호출에서 마지막 문장까지 이미 출력했으므로 더 출력할 텍스트가 없습니다.
                OnCompleted?.Invoke();
                return false;
            }

            ApplyTextVisibility();

            if (speakerText != null)
            {
                speakerText.text = this.dialogue.speakerName;
            }

            if (dialogueText != null)
            {
                dialogueText.maxVisibleCharacters = int.MaxValue;
            }

            int currentIndex = this.dialogue.index;
            ShowCurrentTextAsync(this.dialogue, currentIndex).Forget();
            this.dialogue.index++;

            // 현재 index의 문장을 새로 출력했으므로 이번 호출은 출력 처리에 성공했습니다.
            return true;
        }

        /// <summary>
        /// 인스펙터 설정에 따라 화자 이름과 대사 본문 표시 여부를 반영합니다.
        /// </summary>
        private void ApplyTextVisibility()
        {
            if (speakerText != null)
            {
                speakerText.gameObject.SetActive(showSpeakerText);
            }

            if (dialogueText != null)
            {
                dialogueText.gameObject.SetActive(showDialogueText);
            }
        }

        /// <summary>
        /// 현재 Dialouge.index에 해당하는 문장을 한 글자씩 출력합니다.
        /// </summary>
        private async UniTaskVoid ShowCurrentTextAsync(Dialouge dialouge, int index)
        {
            CancelTypingTask();
            typingCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());

            int currentTypingVersion = ++typingVersion;
            isTyping = true;

            if (dialogueText == null || dialouge == null || dialouge.texts == null || index < 0 || index >= dialouge.texts.Count)
            {
                isTyping = false;
                DisposeTypingTask();
                return;
            }

            string text = dialouge.texts[index];

            // 전체 문장을 먼저 배치한 뒤 보이는 글자 수만 늘려 줄바꿈 위치가 흔들리지 않게 합니다.
            dialogueText.text = text;
            dialogueText.maxVisibleCharacters = 0;
            dialogueText.ForceMeshUpdate();

            int visibleCount = dialogueText.textInfo.characterCount;

            try
            {
                for (int i = 0; i <= visibleCount; i++)
                {
                    if (currentTypingVersion != typingVersion) return;

                    dialogueText.maxVisibleCharacters = i;

                    await UniTask.Delay(
                        System.TimeSpan.FromSeconds(typeInterval),
                        cancellationToken: typingCancellationTokenSource.Token);
                }
            }
            catch (System.OperationCanceledException)
            {
                // UI 파괴 또는 현재 문장 즉시 완성으로 타자기 효과가 중단될 때 발생하는 정상 취소입니다.
                return;
            }

            if (currentTypingVersion != typingVersion) return;

            isTyping = false;
            DisposeTypingTask();
        }

        /// <summary>
        /// 타자기 출력 중 버튼을 다시 눌렀을 때 현재 문장을 즉시 전부 표시합니다.
        /// </summary>
        private void CompleteTyping()
        {
            // 현재 타자기 루프를 실제로 취소하고, 표시 중인 문장만 즉시 끝까지 보여줍니다.
            CancelTypingTask();
            typingVersion++;

            if (dialogueText != null)
            {
                dialogueText.maxVisibleCharacters = int.MaxValue;
            }

            isTyping = false;
        }

        private void CancelTypingTask()
        {
            if (typingCancellationTokenSource == null) return;

            typingCancellationTokenSource.Cancel();
            DisposeTypingTask();
        }

        private void DisposeTypingTask()
        {
            if (typingCancellationTokenSource == null) return;

            typingCancellationTokenSource.Dispose();
            typingCancellationTokenSource = null;
        }
    }
}
