using DG.Tweening;
using UnityEngine;

namespace TRPG.Runtime
{
    public class DOTweenAnimationGroup : MonoBehaviour
    {
        [SerializeField] private DOTweenAnimation[] animations;

        public DOTweenAnimation[] Animations => animations ?? System.Array.Empty<DOTweenAnimation>();

        private void Reset()
        {
            Refresh();
        }

        public void Play()
        {
            foreach (DOTweenAnimation animation in Animations)
            {
                if (animation == null) continue;

                // DOTweenAnimation.DOPlay only works after a tween exists, so recreate before playing.
                animation.RewindThenRecreateTweenAndPlay();
            }
        }

        public void Stop()
        {
            foreach (DOTweenAnimation animation in Animations)
            {
                if (animation == null) continue;

                // Stop returns grouped animations to their initial state.
                animation.DORewind();
            }
        }

        public void Refresh()
        {
            animations = GetComponentsInChildren<DOTweenAnimation>(true);
        }
    }
}
