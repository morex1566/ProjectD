using UnityEngine;

namespace TRPG.Runtime
{
    [RootGameObjectOnly]
    [RequireComponent(typeof(RectTransform))]
    public abstract class UIBase : MonoBehaviour
    {
        public RectTransform rectTransform { get; set; }

        public RectTransform rectTransformParent;

        protected virtual void Awake()
        {
            rectTransform = transform as RectTransform;
            rectTransformParent = rectTransform.parent as RectTransform;
        }
    }
}
