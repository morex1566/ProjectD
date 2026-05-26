using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace TRPG.Runtime
{
    /// <summary>
    /// 월드 좌표 기준으로 선택 가능한 오브젝트를 찾고 선택 상태를 관리합니다.
    /// </summary>
    public class ObjectSelector2D : MonoBehaviour
    {
        [SerializeField] public GameObject cursor;

        public UnityEvent<ISelectable> OnSelectableSelected = new UnityEvent<ISelectable>();

        public UnityEvent<ISelectable> OnSelectableDeselected = new UnityEvent<ISelectable>();

        public ISelectable SelectedSelectable { get; private set; }

        public CreatureController SelectedCreature => SelectedSelectable as CreatureController;



        /// <summary>
        /// 현재 마우스의 위치에 Ground 타일 있음? 있으면 커서 cellPos 이동
        /// </summary>
        private void Update()
        {
            Vector3 mouseWorldPos = MouseEx.GetMouseWorldPos(Camera.main);

            if (WorldManager.GetInstance().TryGetGroundCellPos(mouseWorldPos, out Vector3Int cellPos))
            {
                WorldManager.GetInstance().TryGetGroundWorldPos(cellPos, out Vector3 worldPos);

                cursor.SetActive(true);
                cursor.transform.position = worldPos;
            }
            else
            {
                cursor.SetActive(false);
            }
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
    }
}
