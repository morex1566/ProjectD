using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// Inspector에서 필드를 읽기 전용으로 표시하기 위한 커스텀 속성입니다.
    /// </summary>
    public class ReadOnlyAttribute : PropertyAttribute
    {
        /// <summary>
        /// Unity PropertyDrawer가 하위 프로퍼티까지 읽기 전용으로 그리도록 속성을 생성합니다.
        /// </summary>
        public ReadOnlyAttribute() : base(true)
        {
        }
    }
}
