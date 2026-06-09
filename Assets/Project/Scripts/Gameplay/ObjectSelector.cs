using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace TRPG.Runtime
{
    /// <summary>
    /// InputManager의 포인터 입력을 받아 클릭 선택, 드래그 선택, 선택 대상 이동 명령을 처리합니다.
    /// </summary>
    public class ObjectSelector : MonoBehaviour
    {
        /// <summary>
        /// 드래깅이 너무 작으면 개별 클릭으로 전환되는데 그 기준 길이
        /// </summary>
        [SerializeField] private float dragThreshold = 8f;

        /// <summary>
        /// 드래그 대상
        /// </summary>
        private readonly List<ISelectable> selectedInsts = new();

        /// <summary>
        /// 드래그 시작지점 캐싱
        /// </summary>
        private Vector2 startPointerDownScreenPos;

        /// <summary>
        /// 선택중?
        /// </summary>
        private bool isPointerDown;


        private void OnEnable()
        {
            InputManager.InputMappingContext.Player.LeftClick.performed += OnLeftClickStarted;
            InputManager.InputMappingContext.Player.LeftClick.canceled += OnLeftClickCanceled;
        }

        private void OnDisable()
        {
            InputManager.InputMappingContext.Player.LeftClick.performed -= OnLeftClickStarted;
            InputManager.InputMappingContext.Player.LeftClick.canceled -= OnLeftClickCanceled;
        }

        private void OnLeftClickStarted(InputAction.CallbackContext context)
        {
            // 드래그 시작지점이 UI 위치임?
            if (IsPointerOverUI()) return;

            startPointerDownScreenPos = Pointer.current.position.ReadValue();
            isPointerDown = true;
        }

        private void OnLeftClickCanceled(InputAction.CallbackContext context)
        {
            // 이전 제약조건에 의해 드래깅 취소 된거였음?
            if (!isPointerDown) return;

            isPointerDown = false;

            // 드래그 종료지점이 UI 위치임?
            if (IsPointerOverUI()) return;

            Vector2 endPointerDownScreenPos = Pointer.current.position.ReadValue();
            Vector2 endPointerDownWorldPos = MouseEx.GetMouseWorldPos(WorldManager.CamController.Cam);

            float dragSqrDistance = (endPointerDownScreenPos - startPointerDownScreenPos).sqrMagnitude;
            if (dragSqrDistance >= dragThreshold * dragThreshold)
            {
                Selects(startPointerDownScreenPos, endPointerDownScreenPos);
            }
            else
            {
                Select(endPointerDownWorldPos);
            }
        }

        private static bool IsPointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        private void Selects(Vector2 startPos, Vector2 endPos)
        {
            Rect selectionRect = CreateScreenRect(startPos, endPos);
            MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            Camera cam = WorldManager.CamController.Cam;

            selectedInsts.Clear();

            for (int i = 0; i < behaviours.Length; i++)
            {
                // MonoBehaviour 중에서 ISelectable을 구현한 것만 통과.
                if (behaviours[i] is not ISelectable selectable) continue;

                // 선택 가능한 상태인지?
                if (!selectable.CanSelect) continue;

                // 드래깅 범위에 있음?
                if (!IsBoundsInScreenRect(cam, selectable.SelectionBounds, selectionRect)) continue;

                selectedInsts.Add(selectable);
            }
        }

        private static Rect CreateScreenRect(Vector2 startPos, Vector2 endPos)
        {
            Vector2 min = Vector2.Min(startPos, endPos);
            Vector2 max = Vector2.Max(startPos, endPos);

            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        private static bool IsBoundsInScreenRect(Camera camera, Bounds bounds, Rect screenRect)
        {
            if (camera == null) return false;

            Vector3 min = bounds.min;
            Vector3 max = bounds.max;

            Vector3 screenMin = camera.WorldToScreenPoint(new Vector3(min.x, min.y, bounds.center.z));
            Vector3 screenMax = camera.WorldToScreenPoint(new Vector3(max.x, max.y, bounds.center.z));
            Rect boundsRect = CreateScreenRect(screenMin, screenMax);

            return screenRect.Overlaps(boundsRect) || screenRect.Contains(camera.WorldToScreenPoint(bounds.center));
        }

        private void Select(Vector2 mouseWorldPos)
        {
            selectedInsts.Clear();

            MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            Camera cam = WorldManager.CamController.Cam;
            ISelectable bestSelectable = null;
            float bestSqrDistance = float.MaxValue;
            int bestInstanceId = int.MaxValue;

            for (int i = 0; i < behaviours.Length; i++)
            {
                // MonoBehaviour 중에서 ISelectable을 구현한 것만 통과.
                if (behaviours[i] is not ISelectable selectable) continue;

                // 선택 가능한 상태인지?
                if (!selectable.CanSelect) continue;

                // 대상이 클릭 가능한 위치였음?
                if (!selectable.Contains(mouseWorldPos)) continue;

                // 선택
                // 1. 가장 가까운 대상
                // 2. 인스턴스ID가 가장 작은 대상
                float sqrDistance = Vector3.SqrMagnitude(cam.transform.position - selectable.SelectionBounds.center);
                int instanceId = behaviours[i].GetInstanceID();
                if (sqrDistance < bestSqrDistance || (Mathf.Approximately(sqrDistance, bestSqrDistance) && instanceId < bestInstanceId))
                {
                    bestSelectable = selectable;
                    bestSqrDistance = sqrDistance;
                    bestInstanceId = instanceId;
                }
            }

            if (bestSelectable is not null) selectedInsts.Add(bestSelectable);
        }
    }
}
