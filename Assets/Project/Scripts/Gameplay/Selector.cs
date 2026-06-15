using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TRPG.Runtime
{
    public enum SelectionMode
    {
        Object,
        Construction
    }

    /// <summary>
    /// 입력과 선택 UI를 소유하고, 현재 SelectionMode에 맞는 선택기로 작업을 위임합니다.
    /// </summary>
    public class Selector : MonoBehaviour
    {
        [SerializeField] public SelectionMode Mode = SelectionMode.Object;

        /// <summary>
        /// 드래깅이 너무 작으면 개별 클릭으로 전환되는데 그 기준 길이
        /// </summary>
        [SerializeField] private float dragThreshold = 8f;

        /// <summary>
        /// 드래그 선택 영역을 표시할 UI 프리팹입니다.
        /// </summary>
        [SerializeField] private RectTransform selectionBoxPf = null;

        [SerializeField] private ObjectSelector objectSelector = new();

        [SerializeField] private TileSelector tileSelector = new();

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

        public IReadOnlyList<ISelectable> SelectedInsts => objectSelector.SelectedInsts;


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

            // 드래그가 너무 작으면 SelectionBox를 보이지 않음
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
            if (ScreenEx.IsPointerOverUI(pointerScreenPos)) return;

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
            if (ScreenEx.IsPointerOverUI(pointerScreenPos)) return;

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

        private void Selects(Vector2 startPos, Vector2 endPos)
        {
            switch (Mode)
            {
                case SelectionMode.Object:
                    objectSelector.Selects(startPos, endPos);
                    break;

                case SelectionMode.Construction:
                    tileSelector.Selects(startPos, endPos);
                    break;
            }
        }

        private void Select(Vector2 mouseWorldPos)
        {
            switch (Mode)
            {
                case SelectionMode.Object:
                    objectSelector.Select(mouseWorldPos);
                    break;

                case SelectionMode.Construction:
                    tileSelector.Select(mouseWorldPos);
                    break;
            }
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
