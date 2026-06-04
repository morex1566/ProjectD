using Cysharp.Threading.Tasks;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// EventManager가 GameObject 프리팹으로 생성하고 실행하는 이벤트의 기본 타입입니다.
    /// </summary>
    public class Event : MonoBehaviour
    {
        protected UniTaskCompletionSource eventCompletionSource = null;

        /// <summary>
        /// 이벤트 실행 진입점입니다. 파생 이벤트는 필요한 비동기 흐름을 이 메서드에 작성합니다.
        /// </summary>
        public virtual UniTask ExecuteAsync()
        {
            return eventCompletionSource.Task;
        }
    }
}
