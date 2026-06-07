using DG.Tweening;
using System;
using UnityEngine;

namespace TRPG.Runtime
{
    public class DOTweenAnimationGroup : MonoBehaviour
    {
        [SerializeField] private DOTweenAnimation[] anims = System.Array.Empty<DOTweenAnimation>();

        [SerializeField] private DOTweenAnimation[] reverseAnims = System.Array.Empty<DOTweenAnimation>();

        public DOTweenAnimation[] Anims => anims ?? System.Array.Empty<DOTweenAnimation>();

        public DOTweenAnimation[] ReverseAnims => reverseAnims ?? System.Array.Empty<DOTweenAnimation>();

        public Action OnReverseAnimEvtComplete = null;

        private void Reset()
        {
            Refresh();
        }

        public void PlayAnim()
        {
            foreach (DOTweenAnimation anim in anims)
            {
                if (anim == null) continue;

                // isFrom 애니메이션은 Rewind 후 재생성하면 From 값이 새 도착값으로 잡힐 수 있으므로 현재 프리팹 배치값을 보존합니다.
                anim.RecreateTweenAndPlay();
            }
        }

        /// <summary>
        /// 애니메이션이 끝나면 이 객체가 삭제됩니다.
        /// </summary>
        public void PlayReverseAnimEvt()
        {
            DOTweenAnimation[] targets = ReverseAnims.Length > 0 ? ReverseAnims : Anims;
            foreach (DOTweenAnimation anim in targets)
            {
                if (anim == null) continue;

                DOTweenAnimationEx.PlayReverse(anim);

                anim.tween.onComplete += () =>
                {
                    OnReverseAnimEvtComplete?.Invoke();
                };
            }
        }

        public void Stop()
        {
            foreach (DOTweenAnimation anim in anims)
            {
                if (anim == null) continue;

                // Stop returns grouped anims to their initial state.
                anim.DORewind();
            }
        }

        public void Refresh()
        {
            anims = GetComponentsInChildren<DOTweenAnimation>(true);
        }
    }
}
