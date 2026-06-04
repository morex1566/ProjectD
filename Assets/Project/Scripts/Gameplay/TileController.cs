using DG.Tweening;
using UnityEngine;

namespace TRPG.Runtime
{
    public class TileController : MonoBehaviour
    {
        [Header(nameof(CreatureController) + ".Setup")]

        [SerializeField] private SpriteRenderer spriter;

        [SerializeField, ReadOnly] private DOTweenAnimation moveAnim;

        [SerializeField, ReadOnly] private DOTweenAnimation fadeAnim;

        [SerializeField] private Vector3Range moveAnimFromOffsetRange;

        [SerializeField] private FloatRange moveAnimDurationRange;

        [ReadOnly] public Vector3Int CellPos;


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
