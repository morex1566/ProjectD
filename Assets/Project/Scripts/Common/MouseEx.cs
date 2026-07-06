using UnityEngine;
using UnityEngine.InputSystem;

namespace TRPG.Runtime
{
    /// <summary>
    /// 마우스/포인터 화면 좌표를 월드 좌표로 변환하는 유틸리티입니다.
    /// </summary>
    public static class MouseEx
    {
        /// <summary>
        /// 현재 마우스 화면 좌표에 대한 월드 좌표를 가져옵니다.
        /// </summary>
        public static Vector3 GetMouseWorldPos(Camera camera)
        {
            TryGetMouseWorldPos(camera, out Vector3 worldPos);

            return worldPos;
        }

        /// <summary>
        /// 현재 포인터 화면 좌표에 대한 z=0 평면의 월드 좌표를 가져옵니다.
        /// </summary>
        public static bool TryGetMouseWorldPos(Camera camera, out Vector3 worldPos)
        {
            worldPos = default;

            if (Pointer.current == null) return false;

            Vector2 screenPosition = Pointer.current.position.ReadValue();

            return TryGetWorldPos(camera, screenPosition, out worldPos);
        }

        /// <summary>
        /// 지정된 화면 좌표에 대한 z=0 평면의 월드 좌표를 가져옵니다.
        /// </summary>
        public static bool TryGetWorldPos(Camera camera, Vector2 screenPosition, out Vector3 worldPos)
        {
            worldPos = default;

            if (camera == null) return false;

            // 포커스 전환이나 초기 입력 프레임에서 NaN 좌표가 들어오면 해당 입력만 무시합니다.
            if (!IsFinite(screenPosition)) return false;

            if (!IsFinite(screenPosition)) return false;

            Ray ray = camera.ScreenPointToRay(screenPosition);
            Plane worldPlane = new Plane(Vector3.forward, Vector3.zero);

            if (!worldPlane.Raycast(ray, out float distance)) return false;

            worldPos = ray.GetPoint(distance);
            worldPos.z = 0f;

            return true;
        }

        /// <summary>
        /// Vector2의 두 축 모두 유효한 숫자인지 확인합니다.
        /// </summary>
        private static bool IsFinite(Vector2 value)
        {
            return IsFinite(value.x) && IsFinite(value.y);
        }

        /// <summary>
        /// float 값이 NaN이나 Infinity가 아닌지 확인합니다.
        /// </summary>
        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
