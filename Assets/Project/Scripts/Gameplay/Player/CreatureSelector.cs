using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// 오브젝트 클릭 선택과 드래그 선택 로직을 처리합니다.
    /// </summary>
    public class CreatureSelector : Selector<ISelectable>
    {
        /// <summary>
        /// 오브젝트 선택 모드 진입 시 이전 선택 상태를 비웁니다.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            Clear();
        }

        /// <summary>
        /// 오브젝트 선택 모드 종료 시 선택 표시를 해제합니다.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            Clear();
        }

        /// <summary>
        /// 드래그 사각형 안에 들어온 선택 가능 오브젝트를 모두 선택합니다.
        /// </summary>
        protected override void Selects(Camera cam, Vector2 startPos, Vector2 endPos)
        {
            Clear();

            Rect selectionRect = ScreenEx.CreateScreenRect(startPos, endPos);
            MonoBehaviour[] behaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

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

        /// <summary>
        /// 클릭 위치를 포함하는 선택 가능 오브젝트 중 카메라에 가장 가까운 대상을 선택합니다.
        /// </summary>
        protected override void Select(Camera cam, Vector2 mouseWorldPos)
        {
            Clear();

            MonoBehaviour[] behaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
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

        /// <summary>
        /// 현재 선택 목록과 각 대상의 선택 상태를 함께 초기화합니다.
        /// </summary>
        protected override void Clear()
        {
            for (int i = 0; i < selecteds.Count; i++)
            {
                // 선택 리스트를 비울 때 실제 선택 상태도 함께 해제합니다.
                selecteds[i].SetSelected(false);
            }

            selecteds.Clear();
        }

        /// <summary>
        /// 선택 목록에 대상을 추가하고 실제 선택 표시를 켭니다.
        /// </summary>
        protected override void Add(ISelectable selectedTarget)
        {
            selecteds.Add(selectedTarget);
            selectedTarget.SetSelected(true);
        }
    }
}
