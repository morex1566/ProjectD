using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
        /// 드래그 선택 영역을 표시할 UI 프리팹입니다.
        /// </summary>
        [SerializeField] private RectTransform selectionBoxPf = null;

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

        private RectTransform selectionBoxCanvasRect;

        private RectTransform selectionBox;


        public IReadOnlyList<ISelectable> SelectedInsts => selectedInsts;


        private void Awake()
        {
            CreateSelectionBox();
            HideSelectionBox();
        }

        private void OnEnable()
        {
            InputManager.InputMappingContext.Player.LeftClick.performed += OnLeftClickStarted;
            InputManager.InputMappingContext.Player.LeftClick.canceled += OnLeftClickCanceled;
        }

        private void OnDisable()
        {
            InputManager.InputMappingContext.Player.LeftClick.performed -= OnLeftClickStarted;
            InputManager.InputMappingContext.Player.LeftClick.canceled -= OnLeftClickCanceled;
            HideSelectionBox();
        }

        private void Update()
        {
            if (!isPointerDown) return;
            if (Pointer.current == null) return;

            Vector2 pointerScreenPos = Pointer.current.position.ReadValue();
            float dragSqrDistance = (pointerScreenPos - startPointerDownScreenPos).sqrMagnitude;
            if (dragSqrDistance < dragThreshold * dragThreshold)
            {
                HideSelectionBox();
                return;
            }

            ShowSelectionBox(startPointerDownScreenPos, pointerScreenPos);
        }

        private void OnLeftClickStarted(InputAction.CallbackContext context)
        {
            if (Pointer.current == null) return;

            Vector2 pointerScreenPos = Pointer.current.position.ReadValue();
            StartSelect(pointerScreenPos);
        }

        private void OnLeftClickCanceled(InputAction.CallbackContext context)
        {
            if (Pointer.current == null) return;

            Vector2 pointerScreenPos = Pointer.current.position.ReadValue();
            EndSelect(pointerScreenPos);
        }

        private void StartSelect(Vector2 pointerScreenPos)
        {
            // 드래그 시작지점이 UI 위치임?
            if (IsPointerOverUI(pointerScreenPos)) return;

            startPointerDownScreenPos = pointerScreenPos;
            isPointerDown = true;
            HideSelectionBox();
        }

        private void EndSelect(Vector2 pointerScreenPos)
        {
            // 이전 제약조건에 의해 드래깅 취소 된거였음?
            if (!isPointerDown)
            {
                HideSelectionBox();
                return;
            }

            isPointerDown = false;
            HideSelectionBox();

            // 드래그 종료지점이 UI 위치임?
            if (IsPointerOverUI(pointerScreenPos)) return;

            Vector2 endPointerDownWorldPos = MouseEx.GetMouseWorldPos(WorldManager.CamController.Cam);

            float dragSqrDistance = (pointerScreenPos - startPointerDownScreenPos).sqrMagnitude;
            if (dragSqrDistance >= dragThreshold * dragThreshold)
            {
                Selects(startPointerDownScreenPos, pointerScreenPos);
            }
            else
            {
                Select(endPointerDownWorldPos);
            }
        }

        private static bool IsPointerOverUI(Vector2 pointerScreenPos)
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

        private void Selects(Vector2 startPos, Vector2 endPos)
        {
            ClearSelection();

            Rect selectionRect = CreateScreenRect(startPos, endPos);
            MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            Camera cam = WorldManager.CamController.Cam;

            for (int i = 0; i < behaviours.Length; i++)
            {
                // MonoBehaviour 중에서 ISelectable을 구현한 것만 통과.
                if (behaviours[i] is not ISelectable selectable) continue;

                // 선택 가능한 상태인지?
                if (!selectable.CanSelect) continue;

                // 드래깅 범위에 있음?
                if (!IsBoundsInScreenRect(cam, selectable.SelectionBounds, selectionRect)) continue;

                AddSelection(selectable);
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
            ClearSelection();

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

            if (bestSelectable is not null) AddSelection(bestSelectable);
        }

        private void ClearSelection()
        {
            for (int i = 0; i < selectedInsts.Count; i++)
            {
                // 선택 리스트를 비울 때 실제 선택 상태도 함께 해제합니다.
                selectedInsts[i].SetSelected(false);
            }

            selectedInsts.Clear();
        }

        private void AddSelection(ISelectable selectable)
        {
            selectedInsts.Add(selectable);
            selectable.SetSelected(true);
        }

        private void CreateSelectionBox()
        {
            if (selectionBox != null) return;
            if (selectionBoxPf == null) return;

            GameObject canvasObj = new GameObject("SelectionBoxCanvas", typeof(RectTransform), typeof(Canvas));
            canvasObj.transform.SetParent(transform, false);

            Canvas canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            selectionBoxCanvasRect = canvasObj.GetComponent<RectTransform>();
            selectionBox = Instantiate(selectionBoxPf, selectionBoxCanvasRect);
            selectionBox.anchorMin = new Vector2(0.5f, 0.5f);
            selectionBox.anchorMax = new Vector2(0.5f, 0.5f);
            selectionBox.pivot = new Vector2(0.5f, 0.5f);

            // 선택 박스 UI가 드래그 종료 지점의 UI 판정을 막지 않게 합니다.
            Graphic selectionBoxGraphic = selectionBox.GetComponent<Graphic>();
            if (selectionBoxGraphic != null) selectionBoxGraphic.raycastTarget = false;
        }

        private void ShowSelectionBox(Vector2 startScreenPos, Vector2 endScreenPos)
        {
            if (selectionBox == null) return;
            if (selectionBoxCanvasRect == null) return;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(selectionBoxCanvasRect, startScreenPos, null, out Vector2 localStart)) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(selectionBoxCanvasRect, endScreenPos, null, out Vector2 localEnd)) return;

            Vector2 min = Vector2.Min(localStart, localEnd);
            Vector2 max = Vector2.Max(localStart, localEnd);

            selectionBox.gameObject.SetActive(true);
            selectionBox.anchoredPosition = (min + max) * 0.5f;
            selectionBox.sizeDelta = max - min;
        }

        private void HideSelectionBox()
        {
            if (selectionBox == null) return;

            selectionBox.gameObject.SetActive(false);
        }
    }
}
