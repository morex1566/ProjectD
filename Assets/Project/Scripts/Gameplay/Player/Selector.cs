using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace TRPG.Runtime
{
    /// <summary>
    /// 클릭과 드래그 선택 입력을 처리하고 실제 대상 판정은 자식 선택기에 위임합니다.
    /// </summary>
    public abstract class Selector<T> : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float dragThreshold = 8f;

        [SerializeField] private RectTransform selectionBoxPrefab = null;

        private Camera cam = null;

        private RectTransform selectionBoxCanvasRect = null;

        private RectTransform selectionBox = null;

        private Vector2 startPointerDownScreenPosition;

        private bool isPointerDown = false;

        protected readonly List<T> selected = new();


        public virtual IReadOnlyList<T> Selecteds => selected;

        public event Action SelectionCompleted;


        /// <summary>
        /// 선택 박스 UI와 선택에 사용할 월드 카메라를 준비합니다.
        /// </summary>
        protected virtual void Awake()
        {
            CreateSelectionBox();
            HideSelectionBox();
            CacheCamera();
        }

        /// <summary>
        /// 좌클릭 시작과 종료 입력을 선택 처리 함수에 연결합니다.
        /// </summary>
        protected virtual void OnEnable()
        {
            if (InputManager.TryGetInputMappingContext(out InputMappingContext inputMappingContext) == false)
            {
                return;
            }

            inputMappingContext.Player.LeftClick.performed += OnLeftClickStarted;
            inputMappingContext.Player.LeftClick.canceled += OnLeftClickCanceled;
        }

        /// <summary>
        /// 입력 연결과 남아 있는 선택 상태를 정리합니다.
        /// </summary>
        protected virtual void OnDisable()
        {
            if (InputManager.TryGetInputMappingContext(out InputMappingContext inputMappingContext) == true)
            {
                inputMappingContext.Player.LeftClick.performed -= OnLeftClickStarted;
                inputMappingContext.Player.LeftClick.canceled -= OnLeftClickCanceled;
            }

            isPointerDown = false;
            Clear();
            HideSelectionBox();
        }

        /// <summary>
        /// 클릭 홀드 또는 드래그 중인 선택 UI를 갱신합니다.
        /// </summary>
        protected virtual void Update()
        {
            if (isPointerDown == false)
            {
                HideSelectionBox();
                return;
            }

            if (Pointer.current == null || cam == null)
            {
                isPointerDown = false;
                Clear();
                HideSelectionBox();
                return;
            }

            Vector2 pointerScreenPosition = Pointer.current.position.ReadValue();
            float dragSqrDistance = (pointerScreenPosition - startPointerDownScreenPosition).sqrMagnitude;
            if (dragSqrDistance < dragThreshold * dragThreshold)
            {
                Clear();
                HideSelectionBox();
                return;
            }

            ShowSelectionBox(startPointerDownScreenPosition, pointerScreenPosition);
            Selects(cam, startPointerDownScreenPosition, pointerScreenPosition);
        }

        protected abstract void Selects(Camera cam, Vector2 startScreenPosition, Vector2 endScreenPosition);

        protected abstract void Select(Camera cam, Vector2 pointerWorldPosition);

        protected abstract void CompleteSelection();

        protected virtual void Clear()
        {
            selected.Clear();
        }

        protected virtual void Add(T selected)
        {
            this.selected.Add(selected);
        }

        /// <summary>
        /// 좌클릭 시작 위치를 선택 시작점으로 저장합니다.
        /// </summary>
        private void OnLeftClickStarted(InputAction.CallbackContext context)
        {
            if (Pointer.current == null)
            {
                return;
            }

            StartSelect(Pointer.current.position.ReadValue());
        }

        /// <summary>
        /// 좌클릭 종료 위치에서 단일 선택 또는 드래그 선택을 확정합니다.
        /// </summary>
        private void OnLeftClickCanceled(InputAction.CallbackContext context)
        {
            if (Pointer.current == null)
            {
                isPointerDown = false;
                Clear();
                HideSelectionBox();
                return;
            }

            EndSelect(Pointer.current.position.ReadValue());
        }

        /// <summary>
        /// UI가 아닌 위치에서 시작된 입력만 선택 상태로 전환합니다.
        /// </summary>
        private void StartSelect(Vector2 pointerScreenPosition)
        {
            if (ScreenEx.IsPointerOverUI(pointerScreenPosition) == true)
            {
                return;
            }

            if (cam == null)
            {
                CacheCamera();
            }

            Clear();
            startPointerDownScreenPosition = pointerScreenPosition;
            isPointerDown = true;
        }

        /// <summary>
        /// 포인터 이동 거리에 따라 단일 선택과 드래그 선택으로 분기합니다.
        /// </summary>
        private void EndSelect(Vector2 pointerScreenPosition)
        {
            if (isPointerDown == false)
            {
                return;
            }

            isPointerDown = false;
            HideSelectionBox();

            if (ScreenEx.IsPointerOverUI(pointerScreenPosition) == true)
            {
                Clear();
                return;
            }

            if (cam == null)
            {
                Clear();
                return;
            }

            // 드래그 수준
            float dragSqrDistance = (pointerScreenPosition - startPointerDownScreenPosition).sqrMagnitude;
            if (dragSqrDistance >= dragThreshold * dragThreshold)
            {
                Selects(cam, startPointerDownScreenPosition, pointerScreenPosition);
            }
            // 클릭 수준
            else 
            if (MouseEx.TryGetWorldPosition(cam, pointerScreenPosition, out Vector3 pointerWorldPosition) == true)
            {
                Select(cam, pointerWorldPosition);
            }

            CompleteSelection();
            SelectionCompleted?.Invoke();
        }

        /// <summary>
        /// 현재 월드 카메라를 선택 처리용으로 캐싱합니다.
        /// </summary>
        private void CacheCamera()
        {
            WorldCameraController cameraController = WorldManager.GetWorldCameraController();
            cam = cameraController != null ? cameraController.Cam : null;
        }

        /// <summary>
        /// 선택 영역을 표시할 Screen Space Canvas와 선택 박스를 생성합니다.
        /// </summary>
        private void CreateSelectionBox()
        {
            if (selectionBox != null || selectionBoxPrefab == null)
            {
                return;
            }

            GameObject canvasObject = new GameObject("SelectionBoxCanvas", typeof(RectTransform), typeof(Canvas));
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            selectionBoxCanvasRect = canvasObject.GetComponent<RectTransform>();
            selectionBox = Instantiate(selectionBoxPrefab, selectionBoxCanvasRect);
            selectionBox.anchorMin = new Vector2(0.5f, 0.5f);
            selectionBox.anchorMax = new Vector2(0.5f, 0.5f);
            selectionBox.pivot = new Vector2(0.5f, 0.5f);

            Graphic selectionBoxGraphic = selectionBox.GetComponent<Graphic>();
            if (selectionBoxGraphic != null)
            {
                selectionBoxGraphic.raycastTarget = false;
            }
        }

        /// <summary>
        /// 드래그 시작과 종료 화면 좌표 사이에 선택 박스를 표시합니다.
        /// </summary>
        private void ShowSelectionBox(Vector2 startScreenPosition, Vector2 endScreenPosition)
        {
            if (selectionBox == null || selectionBoxCanvasRect == null)
            {
                return;
            }

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(selectionBoxCanvasRect, startScreenPosition, null, out Vector2 localStart) == false ||
                RectTransformUtility.ScreenPointToLocalPointInRectangle(selectionBoxCanvasRect, endScreenPosition, null, out Vector2 localEnd) == false)
            {
                return;
            }

            Vector2 minimum = Vector2.Min(localStart, localEnd);
            Vector2 maximum = Vector2.Max(localStart, localEnd);

            selectionBox.gameObject.SetActive(true);
            selectionBox.anchoredPosition = (minimum + maximum) * 0.5f;
            selectionBox.sizeDelta = maximum - minimum;
        }

        /// <summary>
        /// 선택 박스 오브젝트를 숨깁니다.
        /// </summary>
        private void HideSelectionBox()
        {
            if (selectionBox != null)
            {
                selectionBox.gameObject.SetActive(false);
            }
        }
    }
}
