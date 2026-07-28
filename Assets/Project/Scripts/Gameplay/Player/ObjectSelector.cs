using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TRPG.Runtime
{
    public class ObjectSelector : MonoBehaviour
    {
        [SerializeField, ReadOnly] private GameObject selectionBoxInstance;

        [SerializeField, ReadOnly] private List<CreatureController> selectedCreatures = new();

        [SerializeField, ReadOnly] private List<WorldTile> selectedTiles = new();

        [SerializeField] private GameObject selectionBoxPrefab;

        private Pointer selectionPointer;

        private Canvas selectionCanvas;

        private RectTransform selectionBoxRectTransform;

        private RectTransform selectionCanvasRectTransform;

        private Vector2 selectionStartScreenPosition;

        private bool isDragging;


        private void OnEnable()
        {
            if (InputManager.TryGetInputMappingContext(out InputMappingContext context) == true)
            {
                context.Player.LeftClick.performed += OnLeftClickPerformed;
                context.Player.LeftClick.canceled += OnLeftClickCanceled;
            }

            Clear();
        }

        private void OnDisable()
        {
            if (InputManager.TryGetInputMappingContext(out InputMappingContext context) == true)
            {
                context.Player.LeftClick.performed -= OnLeftClickPerformed;
                context.Player.LeftClick.canceled -= OnLeftClickCanceled;
            }

            Clear();
        }

        private void Start()
        {
            selectionCanvas = GameObject.FindWithTag(UnityConstant.Tags.OverlayCanvas).GetComponent<Canvas>();
            selectionBoxInstance = Instantiate(selectionBoxPrefab, selectionCanvas.transform, false);
            selectionBoxRectTransform = selectionBoxInstance.GetComponent<RectTransform>();
            selectionCanvasRectTransform = selectionBoxRectTransform.parent as RectTransform;

            Vector2 parentPivot = selectionCanvasRectTransform.pivot;

            selectionBoxRectTransform.anchorMin = parentPivot;
            selectionBoxRectTransform.anchorMax = parentPivot;
            selectionBoxRectTransform.pivot = Vector2.zero;

            selectionBoxInstance.SetActive(false);
        }

        private void Update()
        {
            if (isDragging == false || selectionPointer == null)
            {
                return;
            }

            UpdateSelectionBox(Pointer.current.position.ReadValue());
        }

        private void UpdateSelectionBox(Vector2 pointerScreenPosition)
        {
            Rect screenRect = ScreenEx.CreateScreenRect(selectionStartScreenPosition, pointerScreenPosition);
            Camera uiCamera = selectionCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : selectionCanvas.worldCamera;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(selectionCanvasRectTransform, screenRect.min, uiCamera, out Vector2 localMin) == false)
            {
                return;
            }

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(selectionCanvasRectTransform, screenRect.max, uiCamera, out Vector2 localMax) == false)
            {
                return;
            }

            selectionBoxRectTransform.anchoredPosition = localMin;
            selectionBoxRectTransform.sizeDelta = localMax - localMin;
        }

        /// <summary>
        /// UI가 아닌 월드 클릭에서 Creature를 탐색합니다.
        /// </summary>
        private void OnLeftClickPerformed(InputAction.CallbackContext context)
        {
            selectionPointer = context.control.device as Pointer;

            if (selectionPointer == null)
            {
                return;
            }

            Vector2 pointerScreenPosition = selectionPointer.position.ReadValue();

            if (ScreenEx.IsPointerOverUI(pointerScreenPosition) == true)
            {
                selectionPointer = null;
                return;
            }

            selectionStartScreenPosition = pointerScreenPosition;
            UpdateSelectionBox(pointerScreenPosition);

            isDragging = true;
            selectionBoxInstance.SetActive(true);
        }

        private void OnLeftClickCanceled(InputAction.CallbackContext context)
        {
            if (isDragging == false)
            {
                return;
            }

            if (selectionPointer != null)
            {
                UpdateSelectionBox(selectionPointer.position.ReadValue());
            }

            isDragging = false;
            selectionPointer = null;
            selectionBoxInstance.SetActive(false);
        }

        /// <summary>
        /// 기존 선택을 해제하고 전달된 Creature를 선택합니다.
        /// </summary>
        private void Select()
        {

        }

        private void Selects()
        {

        }

        /// <summary>
        /// 현재 Creature 선택을 해제합니다.
        /// </summary>
        private void Clear()
        {
            selectedCreatures.Clear();
            selectedTiles.Clear();
        }
    }
}
