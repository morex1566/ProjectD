using UnityEngine;
using UnityEngine.InputSystem;

namespace TRPG.Runtime
{
    public static class MouseEx
    {
        private const bool UseCrtScreenCorrection = true;

        private const float CrtCurveStrength = 0.1f;

        private const float CrtCurveFalloff = 4f;

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

            if (camera == null) return false;
            if (Pointer.current == null) return false;

            Vector2 screenPosition = Pointer.current.position.ReadValue();
            // 포커스 전환이나 초기 입력 프레임에서 NaN 좌표가 들어오면 해당 입력만 무시합니다.
            if (!IsFinite(screenPosition)) return false;
            if (UseCrtScreenCorrection && !TryApplyCrtScreenCorrection(camera, screenPosition, out screenPosition)) return false;
            if (!IsFinite(screenPosition)) return false;

            Ray ray = camera.ScreenPointToRay(screenPosition);
            Plane worldPlane = new Plane(Vector3.forward, Vector3.zero);

            if (!worldPlane.Raycast(ray, out float distance)) return false;

            worldPos = ray.GetPoint(distance);
            worldPos.z = 0f;

            return true;
        }

        /// <summary>
        /// CRT Fullscreen 셰이더가 최종 화면을 휘게 만든 만큼 클릭 좌표를 원본 렌더 좌표로 보정합니다.
        /// </summary>
        private static bool TryApplyCrtScreenCorrection(Camera camera, Vector2 screenPosition, out Vector2 correctedScreenPosition)
        {
            correctedScreenPosition = screenPosition;

            Rect pixelRect = camera.pixelRect;
            if (!IsFinite(pixelRect.width) || !IsFinite(pixelRect.height)) return false;
            if (pixelRect.width <= Mathf.Epsilon || pixelRect.height <= Mathf.Epsilon) return false;

            Vector2 viewportPosition = new Vector2(
                (screenPosition.x - pixelRect.x) / pixelRect.width,
                (screenPosition.y - pixelRect.y) / pixelRect.height);
            if (!IsFinite(viewportPosition)) return false;

            Vector2 correctedViewportPosition = ApplyCrtCurve(viewportPosition, pixelRect.width, pixelRect.height);
            if (!IsFinite(correctedViewportPosition)) return false;

            correctedScreenPosition = new Vector2(
                correctedViewportPosition.x * pixelRect.width + pixelRect.x,
                correctedViewportPosition.y * pixelRect.height + pixelRect.y);

            return IsFinite(correctedScreenPosition);
        }

        private static Vector2 ApplyCrtCurve(Vector2 uv, float screenWidth, float screenHeight)
        {
            Vector2 centeredUv = uv * 2f - Vector2.one;
            float aspect = screenWidth / Mathf.Max(screenHeight, 1f);
            Vector2 radialUv = new Vector2(centeredUv.x * aspect, centeredUv.y);
            float maxRadius = new Vector2(aspect, 1f).magnitude;
            float normalizedRadius = Mathf.Clamp01(radialUv.magnitude / maxRadius);
            float curveAmount = Mathf.Pow(normalizedRadius, CrtCurveFalloff) * CrtCurveStrength;

            centeredUv *= 1f + curveAmount;
            centeredUv /= 1f + CrtCurveStrength;
            return centeredUv * 0.5f + Vector2.one * 0.5f;
        }

        private static bool IsFinite(Vector2 value)
        {
            return IsFinite(value.x) && IsFinite(value.y);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
