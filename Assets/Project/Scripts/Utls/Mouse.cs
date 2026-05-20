using UnityEngine;
using UnityEngine.InputSystem;

namespace TRPG.Runtime
{
    public static class MouseEx
    {
        /// <summary>
        /// 현재 마우스 화면 좌표에 대한 월드 좌표를 가져옵니다.
        /// </summary>
        public static Vector3 GetMouseWorldPosition(Camera camera)
        {
            TryGetMouseWorldPosition(camera, out Vector3 worldPosition);

            return worldPosition;
        }

        /// <summary>
        /// 현재 포인터 화면 좌표에 대한 z=0 평면의 월드 좌표를 가져옵니다.
        /// </summary>
        public static bool TryGetMouseWorldPosition(Camera camera, out Vector3 worldPosition)
        {
            worldPosition = default;

            if (camera == null) return false;
            if (Pointer.current == null) return false;

            Vector2 screenPosition = Pointer.current.position.ReadValue();
            Ray ray = camera.ScreenPointToRay(screenPosition);
            Plane worldPlane = new Plane(Vector3.forward, Vector3.zero);

            if (!worldPlane.Raycast(ray, out float distance)) return false;

            worldPosition = ray.GetPoint(distance);
            worldPosition.z = 0f;

            return true;
        }
    }
}
