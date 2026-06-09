using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TRPG.Runtime
{
    /// <summary>
    /// InputManager의 포인터 입력을 받아 클릭 선택, 드래그 선택, 선택 대상 이동 명령을 처리합니다.
    /// </summary>
    public class ObjectSelector : MonoBehaviourSingleton<ObjectSelector>
    {
        [SerializeField] private Camera worldCamera;
        [SerializeField] private float dragThreshold = 8f;

        private readonly List<ISelectable> dragResults = new();

        private Vector2 pointerDownPosition;
        private bool isPointerDown;
        private bool isBlockedByUI;

        private void Awake()
        {
            worldCamera ??= Camera.main;
        }

        private void OnEnable()
        {
            InputManager.LeftClickStarted += OnLeftClickStarted;
            InputManager.LeftClickCanceled += OnLeftClickCanceled;
            InputManager.RightClickStarted += OnRightClickStarted;
        }

        private void OnDisable()
        {
            InputManager.LeftClickStarted -= OnLeftClickStarted;
            InputManager.LeftClickCanceled -= OnLeftClickCanceled;
            InputManager.RightClickStarted -= OnRightClickStarted;
        }

        private void OnLeftClickStarted(Vector2 screenPosition)
        {
            pointerDownPosition = screenPosition;
            isPointerDown = true;
            isBlockedByUI = IsPointerOverUI();
        }

        private void OnLeftClickCanceled(Vector2 screenPosition)
        {
            if (!isPointerDown)
            {
                return;
            }

            isPointerDown = false;

            if (isBlockedByUI || IsPointerOverUI())
            {
                isBlockedByUI = false;
                return;
            }

            float dragSqrDistance = (screenPosition - pointerDownPosition).sqrMagnitude;
            if (dragSqrDistance >= dragThreshold * dragThreshold)
            {
                SelectInScreenRect(pointerDownPosition, screenPosition);
            }
            else
            {
                SelectAtPointerPosition();
            }
        }

        private void OnRightClickStarted(Vector2 screenPosition)
        {
            if (IsPointerOverUI()) return;
            if (!MouseEx.TryGetMouseWorldPos(GetWorldCamera(), out Vector3 worldPosition)) return;

            SelectionManager.GetInstance().MoveSelectedCreatures(worldPosition);
        }

        private void SelectAtPointerPosition()
        {
            if (!MouseEx.TryGetMouseWorldPos(GetWorldCamera(), out Vector3 worldPosition)) return;

            ISelectable selectable = FindSelectable(worldPosition);
            SelectionManager selectionManager = SelectionManager.GetInstance();

            if (selectable == null)
            {
                selectionManager.ClearSelection();
                return;
            }

            selectionManager.SelectSingle(selectable);
        }

        private void SelectInScreenRect(Vector2 startPosition, Vector2 endPosition)
        {
            Rect selectionRect = CreateScreenRect(startPosition, endPosition);
            MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            Camera camera = GetWorldCamera();

            dragResults.Clear();

            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is not ISelectable selectable) continue;
                if (!selectable.CanSelect) continue;
                if (!IsBoundsInScreenRect(camera, selectable.SelectionBounds, selectionRect)) continue;

                dragResults.Add(selectable);
            }

            dragResults.Sort(CompareSelectableInstanceId);
            SelectionManager.GetInstance().SelectMany(dragResults);
        }

        private ISelectable FindSelectable(Vector3 worldPosition)
        {
            MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            Camera camera = GetWorldCamera();
            ISelectable bestSelectable = null;
            float bestSqrDistance = float.MaxValue;
            int bestInstanceId = int.MaxValue;

            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is not ISelectable selectable) continue;
                if (!selectable.CanSelect) continue;
                if (!selectable.Contains(worldPosition)) continue;

                float sqrDistance = camera != null
                    ? Vector3.SqrMagnitude(camera.transform.position - selectable.SelectionBounds.center)
                    : 0f;
                int instanceId = behaviours[i].GetInstanceID();

                if (sqrDistance < bestSqrDistance || (Mathf.Approximately(sqrDistance, bestSqrDistance) && instanceId < bestInstanceId))
                {
                    bestSelectable = selectable;
                    bestSqrDistance = sqrDistance;
                    bestInstanceId = instanceId;
                }
            }

            return bestSelectable;
        }

        private Camera GetWorldCamera()
        {
            worldCamera ??= Camera.main;
            return worldCamera;
        }

        private static Rect CreateScreenRect(Vector2 startPosition, Vector2 endPosition)
        {
            Vector2 min = Vector2.Min(startPosition, endPosition);
            Vector2 max = Vector2.Max(startPosition, endPosition);

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

        private static bool IsPointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        private static int CompareSelectableInstanceId(ISelectable lhs, ISelectable rhs)
        {
            if (lhs is not MonoBehaviour lhsBehaviour) return 1;
            if (rhs is not MonoBehaviour rhsBehaviour) return -1;

            return lhsBehaviour.GetInstanceID().CompareTo(rhsBehaviour.GetInstanceID());
        }
    }
}
