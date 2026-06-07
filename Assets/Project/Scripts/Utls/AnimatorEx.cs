using System.Collections;
using UnityEngine;

namespace TRPG.Runtime
{
    public static class AnimatorEx
    {
        /// <summary>
        /// 지정한 Animator 상태에 진입한 뒤, 해당 상태를 벗어날 때까지 대기합니다.
        /// </summary>
        public static IEnumerator WaitForStateExit(Animator animator, string stateName, int layerIndex = 0)
        {
            if (string.IsNullOrEmpty(stateName))
            {
                yield break;
            }

            yield return WaitForStateExit(animator, Animator.StringToHash(stateName), layerIndex);
        }

        /// <summary>
        /// 지정한 Animator 상태에 진입한 뒤, 해당 상태를 벗어날 때까지 대기합니다.
        /// </summary>
        public static IEnumerator WaitForStateExit(Animator animator, int stateHash, int layerIndex = 0)
        {
            if (animator == null)
            {
                yield break;
            }

            yield return null;

            while (animator != null && animator.isActiveAndEnabled && animator.GetCurrentAnimatorStateInfo(layerIndex).shortNameHash != stateHash)
            {
                yield return null;
            }

            while (animator != null && animator.isActiveAndEnabled && animator.GetCurrentAnimatorStateInfo(layerIndex).shortNameHash == stateHash)
            {
                yield return null;
            }
        }
    }
}
