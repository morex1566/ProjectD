using UnityEngine;

namespace TRPG.Runtime
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class UIBase : MonoBehaviour
    {
        public RectTransform rectTransform { get; set; }

        protected virtual void Awake()
        {
            rectTransform = transform as RectTransform;
        }

        public void Close()
        {
            
        }
    }
}
