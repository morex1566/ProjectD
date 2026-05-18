using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace TRPG.Runtime
{
    public class CreatureSelector : MonoBehaviourSingleton<CreatureSelector>
    {
        [SerializeField] private Camera targetCamera;

        public UnityEvent<ISelectable> OnSelectableSelected = new UnityEvent<ISelectable>();
        public UnityEvent<ISelectable> OnSelectableDeselected = new UnityEvent<ISelectable>();

        public ISelectable SelectedSelectable { get; private set; }
        public CreatureController SelectedCreature => SelectedSelectable as CreatureController;

        public static void Init()
        {
            GetInstance();
        }

        private void Update()
        {
            Pointer pointer = Pointer.current;
            if (pointer == null || !pointer.press.wasPressedThisFrame) return;

            ISelectable selectable = FindSelectable(pointer.position.ReadValue());
            if (selectable == null) return;

            Select(selectable);
        }

        public void Select(ISelectable selectable)
        {
            if (SelectedSelectable == selectable) return;

            Deselect();

            SelectedSelectable = selectable;
            SelectedSelectable.SetSelected(true);
            OnSelectableSelected.Invoke(SelectedSelectable);
        }

        public void Deselect()
        {
            if (SelectedSelectable == null) return;

            ISelectable previousSelectable = SelectedSelectable;
            SelectedSelectable = null;

            previousSelectable.SetSelected(false);
            OnSelectableDeselected.Invoke(previousSelectable);
        }

        private ISelectable FindSelectable(Vector2 screenPosition)
        {
            Camera camera = targetCamera ? targetCamera : Camera.main;
            MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is not ISelectable selectable) continue;
                if (!selectable.CanSelect) continue;
                if (selectable.ContainsScreenPosition(screenPosition, camera)) return selectable;
            }

            return null;
        }
    }
}
