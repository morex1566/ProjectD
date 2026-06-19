using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// RectTransform 기반 UI 컴포넌트의 공통 베이스입니다.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public abstract class UIBase : MonoBehaviour
    {
        public RectTransform rectTransform { get; set; }

        /// <summary>
        /// UI GameObject의 RectTransform을 캐싱합니다.
        /// </summary>
        protected virtual void Awake()
        {
            rectTransform = transform as RectTransform;
        }

        /// <summary>
        /// UI 닫기 동작의 공통 진입점입니다.
        /// </summary>
        public void Close()
        {
            
        }
    }
}
