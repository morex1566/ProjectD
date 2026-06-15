using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TRPG.Runtime
{
    public static class ScreenEx
    {
        /// <summary>
        /// 화면 좌표를 월드 좌표로 변환합니다.
        /// </summary>
        public static Vector3 ScreenToWorldPos(Camera camera, Vector2 screenPos, float worldZ = 0f)
        {
            // 카메라가 없으면 변환 불가
            if (camera == null)
            {
                return Vector3.zero;
            }

            // 화면 좌표에서 월드 z 평면까지의 거리 계산
            float distance = worldZ - camera.transform.position.z;

            // ScreenToWorldPoint는 z에 카메라로부터의 거리가 필요함
            Vector3 worldPos = camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, distance));

            // 2D에서는 보통 z를 고정
            worldPos.z = worldZ;

            return worldPos;
        }

        /// <summary>
        /// 마우스가 UI 위에 있는지?
        /// </summary>
        public static bool IsPointerOverUI(Vector2 pointerScreenPos)
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null) return false;

            // InputAction 콜백 안에서 IsPointerOverGameObject를 호출하면 Unity가 이전 프레임 UI 상태를 사용합니다.
            PointerEventData pointerEventData = new PointerEventData(eventSystem)
            {
                position = pointerScreenPos
            };
            List<RaycastResult> raycastResults = new();
            eventSystem.RaycastAll(pointerEventData, raycastResults);

            return raycastResults.Count > 0;
        }

        /// <summary>
        /// Screen 스페이스에 Rect 만들기
        /// </summary>
        public static Rect CreateScreenRect(Vector2 startPos, Vector2 endPos)
        {
            Vector2 min = Vector2.Min(startPos, endPos);
            Vector2 max = Vector2.Max(startPos, endPos);

            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        /// <summary>
        /// 월드 객체가 Screen Rect에 걸리는지?
        /// </summary>
        public static bool IsWorldObjBoundsInScreenRect(Camera camera, Bounds worldObjBound, Rect screenRect)
        {
            if (camera == null) return false;

            Vector3 min = worldObjBound.min;
            Vector3 max = worldObjBound.max;

            Vector3 screenMin = camera.WorldToScreenPoint(new Vector3(min.x, min.y, worldObjBound.center.z));
            Vector3 screenMax = camera.WorldToScreenPoint(new Vector3(max.x, max.y, worldObjBound.center.z));
            Rect boundsRect = CreateScreenRect(screenMin, screenMax);

            return screenRect.Overlaps(boundsRect) || screenRect.Contains(camera.WorldToScreenPoint(worldObjBound.center));
        }
    }
}
