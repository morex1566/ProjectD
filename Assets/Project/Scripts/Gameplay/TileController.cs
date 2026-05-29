using DG.Tweening;
using UnityEngine;

namespace TRPG.Runtime
{
    public class TileController : MonoBehaviour
    {
        [Header(nameof(CreatureController) + ".Setup")]

        [SerializeField, ReadOnly] private DOTweenAnimation moveAnim;

        [SerializeField, ReadOnly] private DOTweenAnimation fadeAnim;

        [SerializeField] private Vector3Range moveAnimFromOffsetRange;

        [SerializeField] private FloatRange moveAnimDurationRange;


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
            DOTweenAnimation[] anims = GetComponentsInChildren<DOTweenAnimation>();
            foreach (var anim in anims)
            {
                if (anim.animationType == DOTweenAnimation.AnimationType.Move)
                {
                    moveAnim = anim;
                    continue;
                }

                if (anim.animationType == DOTweenAnimation.AnimationType.Fade)
                {
                    fadeAnim = anim;
                    continue;
                }
            }
        }

        public void OnEnable()
        {
            // Scene 기반 맵 편집 중에는 타일 프리팹을 정적으로 배치해야 하므로 생성 애니메이션을 실행하지 않습니다.
            if (!Application.isPlaying) return;

            // Move
            if (moveAnim == null) return;
            moveAnim.isFrom = true;
            moveAnim.endValueV3 = moveAnimFromOffsetRange.Random();
            moveAnim.duration = moveAnimDurationRange.Random();
            moveAnim.DORestart();

            // Fade
            if (fadeAnim == null) return;
            fadeAnim.DORestart();
        }
    }
}
