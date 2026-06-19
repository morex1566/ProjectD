using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 월드 좌표 기반 선택 대상이 구현해야 하는 공통 계약입니다.
    /// </summary>
    public interface ISelectable
    {
        bool CanSelect { get; set; }

        bool IsSelected { get; set; }

        Bounds SelectionBounds { get; }

        /// <summary>
        /// 월드 좌표가 이 선택 가능 개체의 선택 영역 안에 있는지 검사합니다.
        /// </summary>
        bool Contains(Vector3 worldPosition);

        /// <summary>
        /// 현재 선택 상태를 변경합니다.
        /// </summary>
        void SetSelected(bool isSelected);
    }
}
