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

        private Camera cam;

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



        /// <summary>
        /// 선택 박스 UI를 생성하고 초기 상태를 숨김으로 맞춥니다.
        /// </summary>
        protected virtual void Awake()
        {
            CreateSelectionBox();
            HideSelectionBox();

            cam = WorldManager.GetWorldCameraController().Cam;
        }

        /// <summary>
        /// 좌클릭 시작/종료 입력을 선택 처리 이벤트에 연결합니다.
        /// </summary>
        protected virtual void OnEnable()
        {
            if (!InputManager.TryGetInputMappingContext(out InputMappingContext inputMappingContext)) return;

            inputMappingContext.Player.LeftClick.performed += OnLeftClickStarted;
            inputMappingContext.Player.LeftClick.canceled += OnLeftClickCanceled;
        }

        /// <summary>
        /// 입력 이벤트 연결을 해제하고 남아 있는 선택 박스를 숨깁니다.
        /// </summary>
        protected virtual void OnDisable()
        {
            if (InputManager.TryGetInputMappingContext(out InputMappingContext inputMappingContext))
            {
                inputMappingContext.Player.LeftClick.performed -= OnLeftClickStarted;
                inputMappingContext.Player.LeftClick.canceled -= OnLeftClickCanceled;
            }

            HideSelectionBox();
        }

        /// <summary>
        /// 드래그 중일 때 선택 박스 크기와 위치를 갱신합니다.
        /// </summary>
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

        /// <summary>
        /// 좌클릭 시작 시점의 포인터 위치를 선택 시작점으로 기록합니다.
        /// </summary>
        private void OnLeftClickStarted(InputAction.CallbackContext context)
        {
            if (Pointer.current == null) return;

            Vector2 pointerScreenPos = Pointer.current.position.ReadValue();
            StartSelect(pointerScreenPos);
        }

        /// <summary>
        /// 좌클릭 종료 시 드래그 선택 또는 단일 클릭 선택으로 분기합니다.
        /// </summary>
        private void OnLeftClickCanceled(InputAction.CallbackContext context)
        {
            if (Pointer.current == null) return;

            Vector2 pointerScreenPos = Pointer.current.position.ReadValue();
            EndSelect(pointerScreenPos);
        }

        /// <summary>
        /// UI 위에서 시작하지 않은 포인터 입력만 선택 시작 상태로 전환합니다.
        /// </summary>
        private void StartSelect(Vector2 pointerScreenPos)
        {
            // 드래그 시작지점이 UI 위치임?
            if (ScreenEx.IsPointerOverUI(pointerScreenPos)) return;

            startPointerDownScreenPos = pointerScreenPos;
            isPointerDown = true;
            HideSelectionBox();
        }

        /// <summary>
        /// 선택 종료 위치를 검증한 뒤 드래그 선택과 단일 선택 중 하나를 실행합니다.
        /// </summary>
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
                Selects(cam, startPointerDownScreenPos, pointerScreenPos);
            }
            else
            {
                if (!MouseEx.TryGetWorldPos(cam, pointerScreenPos, out Vector3 pointerWorldPos)) return;

                Select(cam, pointerWorldPos);
            }
        }

        /// <summary>
        /// 드래그 선택 결과를 자식 선택기에서 처리합니다.
        /// </summary>
        protected abstract void Selects(Camera cam, Vector2 startPos, Vector2 endPos);

        /// <summary>
        /// 단일 클릭 선택 결과를 자식 선택기에서 처리합니다.
        /// </summary>
        protected abstract void Select(Camera cam, Vector2 mouseWorldPos);

        /// <summary>
        /// 현재 선택된 대상과 표시 상태를 자식 선택기에서 정리합니다.
        /// </summary>
        protected abstract void Clear();

        /// <summary>
        /// 새 선택 대상을 자식 선택기 방식으로 등록합니다.
        /// </summary>
        protected abstract void Add(T selectedTarget);

        /// <summary>
        /// 드래그 영역을 표시할 Screen Space Canvas와 SelectionBox 인스턴스를 만듭니다.
        /// </summary>
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

        /// <summary>
        /// 드래그 시작/종료 스크린 좌표를 Canvas 로컬 Rect로 변환해 SelectionBox를 표시합니다.
        /// </summary>
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

        /// <summary>
        /// SelectionBox GameObject를 비활성화합니다.
        /// </summary>
        private void HideSelectionBox()
        {
            if (selectionBox == null) return;

            selectionBox.gameObject.SetActive(false);
        }
    }
}
