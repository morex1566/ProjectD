using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 현재 선택된 월드 오브젝트 목록을 관리합니다.
    /// </summary>
    public class SelectionManager : MonoBehaviourSingleton<SelectionManager>
    {
        private readonly List<ISelectable> selectedSelectables = new();

        public IReadOnlyList<ISelectable> SelectedSelectables => selectedSelectables;

        public bool HasSelection => selectedSelectables.Count > 0;

        protected override void OnDestroy()
        {
            ClearSelection();
            base.OnDestroy();
        }

        public void SelectSingle(ISelectable selectable)
        {
            ClearSelection();

            if (!IsSelectableAlive(selectable) || !selectable.CanSelect) return;

            selectedSelectables.Add(selectable);
            selectable.SetSelected(true);
        }

        public void SelectMany(List<ISelectable> selectables)
        {
            ClearSelection();

            if (selectables == null) return;

            for (int i = 0; i < selectables.Count; i++)
            {
                ISelectable selectable = selectables[i];
                if (!IsSelectableAlive(selectable) || !selectable.CanSelect) continue;
                if (selectedSelectables.Contains(selectable)) continue;

                selectedSelectables.Add(selectable);
                selectable.SetSelected(true);
            }
        }

        public void ClearSelection()
        {
            for (int i = 0; i < selectedSelectables.Count; i++)
            {
                if (IsSelectableAlive(selectedSelectables[i]))
                {
                    selectedSelectables[i].SetSelected(false);
                }
            }

            selectedSelectables.Clear();
        }

        /// <summary>
        /// 선택된 크리처들에게 동일한 월드 위치 이동 명령을 전달합니다.
        /// </summary>
        public void MoveSelectedCreatures(Vector3 worldPosition)
        {
            for (int i = 0; i < selectedSelectables.Count; i++)
            {
                if (IsSelectableAlive(selectedSelectables[i]) && selectedSelectables[i] is CreatureController creatureController)
                {
                    creatureController.MoveTo(worldPosition);
                }
            }
        }

        private static bool IsSelectableAlive(ISelectable selectable)
        {
            if (selectable == null) return false;

            return selectable is not Object unityObject || unityObject != null;
        }
    }
}
