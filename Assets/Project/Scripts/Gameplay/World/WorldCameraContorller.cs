using UnityEngine;

namespace TRPG.Runtime
{
    public class WorldCameraContorller : MonoBehaviour
    {
        private Camera cam;

        public Camera Cam => cam;

        private void Awake()
        {
            CacheComponents();
        }

        private void CacheComponents()
        {
            cam = gameObject.GetComponentInHierarchy<Camera>();
        }
    }
}
