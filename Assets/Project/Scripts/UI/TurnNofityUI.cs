using System.Collections;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 턴 알림 UI의 표시 시간과 애니메이션을 관리합니다.
    /// </summary>
    public class TurnNofityUI : MonoBehaviour
    {
        [SerializeField, ReadOnly] private Animator animator;

        [SerializeField] private float lifetime;

        private Coroutine Squash;

        /// <summary>
        /// UI가 생성되면 지정된 수명 코루틴을 시작합니다.
        /// </summary>
        private void Start()
        {
            StartCoroutine(SquashCo());
        }

        /// <summary>
        /// 턴 알림 UI가 유지될 시간을 기다립니다.
        /// </summary>
        private IEnumerator SquashCo()
        {
            yield return new WaitForSeconds(lifetime);


        }
    }
}
