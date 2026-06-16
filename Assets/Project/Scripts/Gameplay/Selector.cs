using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TRPG.Runtime
{
    /// <summary>
    /// 입력과 선택 UI를 처리하고, 실제 선택 처리는 자식 선택기에게 위임하는 부모 컴포넌트입니다.
    /// </summary>
    public abstract class Selector<T> : MonoBehaviour
    {
        /// <summary>
        /// 드래깅이 너무 작으면 개별 클릭으로 전환되는데 그 기준 길이입니다.
        /// </summary>
        [SerializeField] private float dragThreshold = 8f;

        /// <summary>
        /// 드래그 선택 영역을 표시할 UI 프리팹입니다.
        /// </summary>
        [SerializeField] private RectTransform selectionBoxPf = null;

        private RectTransform selectionBoxCanvasRect;

        private RectTransform selectionBox;

        /// <summary>
        /// 드래그 시작지점 캐싱
        /// </summary>
        private Vector2 startPointerDownScreenPos;

        /// <summary>
        /// 선택중?
        /// </summary>
        private bool isPointerDown;

        protected List<T> selecteds = new();

        /// <summary>
        /// 오브젝트 선택 결과입니다. 오브젝트 선택기가 아닌 경우 빈 목록을 반환합니다.
        /// </summary>
        public virtual IReadOnlyList<T> Selecteds => selecteds;




        protected virtual void Awake()
        {
            CreateSelectionBox();
            HideSelectionBox();
        }

        protected virtual void OnEnable()
        {
            InputManager.InputMappingContext.Player.LeftClick.performed += OnLeftClickStarted;
            InputManager.InputMappingContext.Player.LeftClick.canceled += OnLeftClickCanceled;
        }

        protected virtual void OnDisable()
        {
            InputManager.InputMappingContext.Player.LeftClick.performed -= OnLeftClickStarted;
            InputManager.InputMappingContext.Player.LeftClick.canceled -= OnLeftClickCanceled;
            HideSelectionBox();
        }

        protected virtual void Update()
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

            float dragSqrDistance = (pointerScreenPos - startPointerDownScreenPos).sqrMagnitude;
            if (dragSqrDistance >= dragThreshold * dragThreshold)
            {
                Selects(startPointerDownScreenPos, pointerScreenPos);
            }
            else
            {
                if (!MouseEx.TryGetWorldPos(WorldManager.CamController.Cam, pointerScreenPos, out Vector3 pointerWorldPos)) return;

                Select(pointerWorldPos);
            }
        }

        /// <summary>
        /// 드래그 선택 결과를 자식 선택기에서 처리합니다.
        /// </summary>
        protected abstract void Selects(Vector2 startPos, Vector2 endPos);

        /// <summary>
        /// 단일 클릭 선택 결과를 자식 선택기에서 처리합니다.
        /// </summary>
        protected abstract void Select(Vector2 mouseWorldPos);

        protected abstract void Clear();

        protected abstract void Add(T selectedTarget);

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
