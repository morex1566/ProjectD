using UnityEngine;

namespace TRPG.Runtime
{
    public interface ISelectable
    {
        bool CanSelect { get; }
        bool IsSelected { get; }

        bool ContainsScreenPosition(Vector2 screenPosition, Camera targetCamera);
        void SetSelected(bool isSelected);
    }
}
