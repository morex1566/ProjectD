using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 오브젝트 클릭 선택과 드래그 선택 로직을 처리합니다.
    /// </summary>
    public class IdleSelector : Selector<ISelectable>
    {
        protected override void OnEnable()
        {
            base.OnEnable();

            Clear();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            Clear();
        }

        protected override void Selects(Vector2 startPos, Vector2 endPos)
        {
            Clear();

            Rect selectionRect = ScreenEx.CreateScreenRect(startPos, endPos);
            MonoBehaviour[] behaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            Camera cam = WorldManager.CamController.Cam;

            for (int i = 0; i < behaviours.Length; i++)
            {
                // MonoBehaviour 중에서 ISelectable을 구현한 것만 통과.
                if (behaviours[i] is not ISelectable selectable) continue;

                // 선택 가능한 상태인지?
                if (!selectable.CanSelect) continue;

                // 드래깅 범위에 있음?
                if (!ScreenEx.IsWorldObjBoundsInScreenRect(cam, selectable.SelectionBounds, selectionRect)) continue;

                Add(selectable);
            }
        }

        protected override void Select(Vector2 mouseWorldPos)
        {
            Clear();

            MonoBehaviour[] behaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
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

            if (bestSelectable is not null) Add(bestSelectable);
        }

        protected override void Clear()
        {
            for (int i = 0; i < selecteds.Count; i++)
            {
                // 선택 리스트를 비울 때 실제 선택 상태도 함께 해제합니다.
                selecteds[i].SetSelected(false);
            }

            selecteds.Clear();
        }

        protected override void Add(ISelectable selectedTarget)
        {
            selecteds.Add(selectedTarget);
            selectedTarget.SetSelected(true);
        }
    }
}
