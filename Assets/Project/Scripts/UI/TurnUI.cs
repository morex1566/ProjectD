using UnityEngine;
using TMPro;
using System;
using System.Collections;

namespace TRPG.Runtime
{
    public class TurnUI : UIBase
    {
        private const string Combat = "전투!";

        private const string End = "종료!";

        [SerializeField] private TMP_Text messageText = null;

        [SerializeField] private DOTweenAnimationGroup animGroup = null;

        [SerializeField] private float duration = 1f;

        private Coroutine playCoroutine = null;

        public event Action OnCompleted;

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

        private void OnEnable()
        {
            Play(Combat);
        }

        private void OnDisable()
        {
            if (playCoroutine == null) return;

            StopCoroutine(playCoroutine);
            playCoroutine = null;
        }

        /// <summary>
        /// 전투 시작 문구를 재생합니다.
        /// </summary>
        public void PlayCombat()
        {
            Play(Combat);
        }

        /// <summary>
        /// 전투 종료 문구를 재생합니다.
        /// </summary>
        public void PlayEnd()
        {
            Play(End);
        }

        private void Play(string message)
        {
            if (messageText != null)
            {
                messageText.text = message;
            }

            if (!gameObject.activeInHierarchy) return;

            if (playCoroutine != null)
            {
                StopCoroutine(playCoroutine);
            }

            playCoroutine = StartCoroutine(PlayCoroutine());
        }

        private IEnumerator PlayCoroutine()
        {
            animGroup?.PlayAnim();

            float openDuration = animGroup != null
                ? DOTweenAnimationEx.GetDuration(animGroup.Anims)
                : 0f;
            if (openDuration > 0f)
            {
                yield return new WaitForSeconds(openDuration);
            }

            if (duration > 0f)
            {
                yield return new WaitForSeconds(duration);
            }

            animGroup?.PlayReverseAnimEvt();

            float reverseDuration = animGroup != null
                ? DOTweenAnimationEx.GetDuration(animGroup.ReverseAnims.Length > 0 ? animGroup.ReverseAnims : animGroup.Anims)
                : 0f;
            if (reverseDuration > 0f)
            {
                yield return new WaitForSeconds(reverseDuration);
            }

            playCoroutine = null;
            OnCompleted?.Invoke();
            Close();
        }

        private void Bind()
        {
            messageText = GetComponentInChildren<TMP_Text>(true);
            animGroup = GetComponent<DOTweenAnimationGroup>();
        }
    }
}
