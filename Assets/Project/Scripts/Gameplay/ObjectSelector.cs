using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace TRPG.Runtime
{
    public partial class ObjectSelector : MonoBehaviour
    {
        [SerializeField] private GameObject selectorCursor;

        public UnityEvent<ISelectable> OnSelectableSelected = new UnityEvent<ISelectable>();

        public UnityEvent<ISelectable> OnSelectableDeselected = new UnityEvent<ISelectable>();

        public UnityEvent<Vector3Int, Vector3> OnCursorMoved = new UnityEvent<Vector3Int, Vector3>();

        public ISelectable SelectedSelectable { get; private set; }

        public CreatureController SelectedCreature => SelectedSelectable as CreatureController;

        private Vector3Int cursorCellPos = new Vector3Int(0, 0);



        protected void Awake()
        {
            selectorCursor.transform.SetParent(null, true);
        }

        private void OnEnable()
        {
            InputManager.InputMappingContext.Player.LeftClick.performed += OnClick;
        }

        private void OnDisable()
        {
            InputManager.InputMappingContext.Player.LeftClick.performed -= OnClick;
        }

        /// <summary>
        /// 전달된 선택 가능 개체를 현재 선택 대상으로 설정합니다.
        /// </summary>
        public void Select(ISelectable selectable)
        {
            Deselect();

            // 같은 개체를 다시 눌렀어?
            if (SelectedSelectable == selectable) return;

            SelectedSelectable = selectable;
            SelectedSelectable.SetSelected(true);
            OnSelectableSelected.Invoke(SelectedSelectable);
        }

        /// <summary>
        /// 현재 선택 대상을 해제하고 해제 이벤트를 발생시킵니다.
        /// </summary>
        public void Deselect()
        {
            // 선택된 대상 없어?
            if (SelectedSelectable == null) return;

            ISelectable previousSelectable = SelectedSelectable;
            previousSelectable.SetSelected(false);
            SelectedSelectable = null;
            OnSelectableDeselected.Invoke(previousSelectable);
        }

        /// <summary>
        /// 월드 좌표를 포함하는 선택 가능 개체를 씬에서 찾습니다.
        /// </summary>
        public ISelectable FindSelectable(Vector3 worldPos)
        {
            MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

            foreach (MonoBehaviour behaviour in behaviours)
            {
                // 애초에 선택 대상 자체가 없음
                if (behaviour is not ISelectable selectable) continue;
             
                // 좌표에 대상 없음
                if (!selectable.Contains(worldPos)) continue;

                // 대상이 선택가능한 녀석이 아님
                if (!selectable.CanSelect) continue;

                return selectable;
            }

            return null;
        }

        public bool TryMoveCursor(Vector3Int cellPos)
        {
            // 타일맵이 아닌곳?
            if (!WorldManager.GetInstance().TryGetGroundWorldPosition(cellPos, out Vector3 worldPos))
            {
                selectorCursor.SetActive(false);
                return false;
            }

            // 현재 커서의 위치를 다시 지정?
            if (cursorCellPos == cellPos)
            {
                selectorCursor.SetActive(false);
                return false;
            }

            selectorCursor.SetActive(true);
            cursorCellPos = cellPos;
            selectorCursor.transform.position = worldPos;
            OnCursorMoved.Invoke(cursorCellPos, worldPos);
            return true;
        }

        public bool TryMoveCursor(Vector3 worldPos)
        {
            // 타일맵이 아닌곳?
            if (!WorldManager.GetInstance().TryGetGroundCellPosition(worldPos, out Vector3Int cellPos))
            {
                selectorCursor.SetActive(false);
                return false;
            }

            // 현재 커서의 위치를 다시 지정?
            if (cursorCellPos == cellPos && selectorCursor.activeSelf)
            {
                selectorCursor.SetActive(false);
                return false;
            }

            // 월드 좌표 변환 실패?
            if (!WorldManager.GetInstance().TryGetGroundWorldPosition(cellPos, out Vector3 worldPosByCell))
            {
                selectorCursor.SetActive(false);
                return false;
            }

            selectorCursor.SetActive(true);
            cursorCellPos = cellPos;
            selectorCursor.transform.position = worldPosByCell;
            OnCursorMoved.Invoke(cursorCellPos, worldPosByCell);
            return true;
        }
    }

    public partial class ObjectSelector : MonoBehaviour
    {
        private void OnClick(InputAction.CallbackContext context)
        {
            Vector3 worldPos = MouseEx.GetMouseWorldPosition(Camera.main);

            TryMoveCursor(worldPos);
        }
    }

}
