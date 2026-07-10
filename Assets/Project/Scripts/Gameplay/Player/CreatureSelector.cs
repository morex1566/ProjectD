using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// Creature 클릭 선택과 드래그 선택 로직을 처리합니다.
    /// </summary>
    public class CreatureSelector : Selector<ISelectable>
    {
        [SerializeField] private Sprite selectedCreatureSprite;

        private readonly List<ISelectable> previewSelecteds = new();

        private readonly Dictionary<ISelectable, GameObject> selectionUIMap = new();


        /// <summary>
        /// Creature 선택 모드 진입 시 이전 선택 상태를 비웁니다.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            Clear();
        }

        /// <summary>
        /// Creature 선택 모드 종료 시 선택 표시를 해제합니다.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            Clear();
        }

        /// <summary>
        /// 드래그 사각형 안에 들어온 선택 가능 Creature를 임시 표시합니다.
        /// </summary>
        protected override void SelectPreviews(Camera cam, Vector2 startPos, Vector2 endPos)
        {
            SetPreviewSelection(FindSelectableObjects(cam, startPos, endPos));
        }

        /// <summary>
        /// 클릭 홀드 중 포인터 아래의 선택 가능 Creature를 임시 표시합니다.
        /// </summary>
        protected override void SelectPreview(Camera cam, Vector2 mouseWorldPosition)
        {
            List<ISelectable> currentPreviewSelecteds = new();
            ISelectable selectable = FindBestSelectable(cam, mouseWorldPosition);
            if (selectable != null)
            {
                currentPreviewSelecteds.Add(selectable);
            }

            SetPreviewSelection(currentPreviewSelecteds);
        }

        /// <summary>
        /// 드래그 사각형 안에 들어온 선택 가능 Creature를 확정 선택합니다.
        /// </summary>
        protected override void Selects(Camera cam, Vector2 startPos, Vector2 endPos)
        {
            Clear();

            List<ISelectable> currentSelecteds = FindSelectableObjects(cam, startPos, endPos);
            for (int i = 0; i < currentSelecteds.Count; i++)
            {
                Add(currentSelecteds[i]);
            }
        }

        /// <summary>
        /// 클릭 위치를 포함하는 선택 가능 Creature 중 카메라에 가장 가까운 대상을 확정 선택합니다.
        /// </summary>
        protected override void Select(Camera cam, Vector2 mouseWorldPosition)
        {
            Clear();

            ISelectable bestSelectable = FindBestSelectable(cam, mouseWorldPosition);
            if (bestSelectable != null)
            {
                Add(bestSelectable);
            }
        }

        /// <summary>
        /// 임시 선택 표시를 지우고 확정 선택 표시를 복원합니다.
        /// </summary>
        protected override void ClearPreview()
        {
            for (int i = 0; i < previewSelecteds.Count; i++)
            {
                RemoveSelectUI(previewSelecteds[i]);
            }

            previewSelecteds.Clear();

            for (int i = 0; i < selecteds.Count; i++)
            {
                SetSelectUI(selecteds[i]);
            }
        }

        /// <summary>
        /// 확정 선택 목록과 표시 상태를 비웁니다.
        /// </summary>
        protected override void Clear()
        {
            for (int i = 0; i < selecteds.Count; i++)
            {
                selecteds[i].SetSelected(false);
                RemoveSelectUI(selecteds[i]);
            }

            selecteds.Clear();
        }

        /// <summary>
        /// 선택 목록에 대상을 추가하고 선택 표시를 생성합니다.
        /// </summary>
        protected override void Add(ISelectable selectedTarget)
        {
            if (selecteds.Contains(selectedTarget) == true)
            {
                return;
            }

            selecteds.Add(selectedTarget);
            selectedTarget.SetSelected(true);
            SetSelectUI(selectedTarget);
        }

        /// <summary>
        /// 드래그 사각형 안에 들어온 선택 가능 Creature 목록을 찾습니다.
        /// </summary>
        private List<ISelectable> FindSelectableObjects(Camera cam, Vector2 startPos, Vector2 endPos)
        {
            List<ISelectable> selectableObjects = new();
            Rect selectionRect = ScreenEx.CreateScreenRect(startPos, endPos);
            MonoBehaviour[] behaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is not ISelectable selectable) continue;
                if (selectable.CanSelect == false) continue;
                if (ScreenEx.IsWorldObjBoundsInScreenRect(cam, selectable.SelectionBounds, selectionRect) == false) continue;

                selectableObjects.Add(selectable);
            }

            return selectableObjects;
        }

        /// <summary>
        /// 클릭 위치를 포함하는 선택 가능 Creature 중 가장 가까운 대상을 찾습니다.
        /// </summary>
        private ISelectable FindBestSelectable(Camera cam, Vector2 mouseWorldPosition)
        {
            MonoBehaviour[] behaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            ISelectable bestSelectable = null;
            float bestSqrDistance = float.MaxValue;
            int bestInstanceId = int.MaxValue;

            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] is not ISelectable selectable) continue;
                if (selectable.CanSelect == false) continue;
                if (selectable.Contains(mouseWorldPosition) == false) continue;

                float sqrDistance = Vector3.SqrMagnitude(cam.transform.position - selectable.SelectionBounds.center);
                int instanceId = behaviours[i].GetInstanceID();
                if (sqrDistance < bestSqrDistance || Mathf.Approximately(sqrDistance, bestSqrDistance) && instanceId < bestInstanceId)
                {
                    bestSelectable = selectable;
                    bestSqrDistance = sqrDistance;
                    bestInstanceId = instanceId;
                }
            }

            return bestSelectable;
        }

        /// <summary>
        /// 현재 프레임의 임시 선택 표시만 교체합니다.
        /// </summary>
        private void SetPreviewSelection(List<ISelectable> currentPreviewSelecteds)
        {
            BeginPreview();

            for (int i = previewSelecteds.Count - 1; i >= 0; i--)
            {
                if (currentPreviewSelecteds.Contains(previewSelecteds[i]) == true)
                {
                    continue;
                }

                RemoveSelectUI(previewSelecteds[i]);
                previewSelecteds.RemoveAt(i);
            }

            for (int i = 0; i < currentPreviewSelecteds.Count; i++)
            {
                ISelectable selectable = currentPreviewSelecteds[i];
                if (previewSelecteds.Contains(selectable) == true)
                {
                    continue;
                }

                previewSelecteds.Add(selectable);
                SetSelectUI(selectable);
            }
        }

        /// <summary>
        /// 임시 선택이 시작되면 확정 선택 표시는 잠시 숨깁니다.
        /// </summary>
        private void BeginPreview()
        {
            for (int i = 0; i < selecteds.Count; i++)
            {
                RemoveSelectUI(selecteds[i]);
            }
        }

        /// <summary>
        /// 발바닥 접점 위치에 selection_creature 표시 오브젝트를 생성하거나 갱신합니다.
        /// </summary>
        private void SetSelectUI(ISelectable selectable)
        {
            if (selectedCreatureSprite == null) return;

            GroundChecker groundChecker = GetOrCreateGroundChecker(selectable);
            if (groundChecker == null) return;

            groundChecker.Init();
            groundChecker.Generate();

            GameObject selectUI = GetOrCreateSelectionUI(selectable, groundChecker.transform);
            selectUI.transform.localPosition = Vector3.zero;
            selectUI.transform.localRotation = Quaternion.identity;
            selectUI.transform.localScale = Vector3.one;

            SpriteRenderer selectionRenderer = selectUI.GetComponent<SpriteRenderer>();
            if (selectionRenderer == null)
            {
                selectionRenderer = selectUI.AddComponent<SpriteRenderer>();
            }

            selectionRenderer.sprite = selectedCreatureSprite;
            ApplySelectionSorting(groundChecker.TargetRenderer, selectionRenderer);
        }

        /// <summary>
        /// selection_creature 표시 오브젝트를 제거합니다.
        /// </summary>
        private void RemoveSelectUI(ISelectable selectable)
        {
            if (selectable == null) return;
            if (selectionUIMap.TryGetValue(selectable, out GameObject selectUI) == false) return;

            selectionUIMap.Remove(selectable);

            if (selectUI != null)
            {
                Destroy(selectUI);
            }
        }

        /// <summary>
        /// 선택 대상이 가진 GroundChecker를 찾습니다.
        /// </summary>
        private GroundChecker GetGroundChecker(ISelectable selectable)
        {
            if (selectable == null) return null;
            if (selectable.SelectedInst == null) return null;

            return selectable.SelectedInst.GetComponentInChildren<GroundChecker>();
        }

        /// <summary>
        /// 선택 대상의 GroundChecker를 가져오거나 없으면 자식으로 생성합니다.
        /// </summary>
        private GroundChecker GetOrCreateGroundChecker(ISelectable selectable)
        {
            if (selectable == null) return null;
            if (selectable.SelectedInst == null) return null;

            GroundChecker groundChecker = selectable.SelectedInst.GetComponentInChildren<GroundChecker>();
            if (groundChecker == null)
            {
                GameObject groundCheckerObj = new GameObject(nameof(GroundChecker));
                groundCheckerObj.transform.SetParent(selectable.SelectedInst.transform, false);
                groundChecker = groundCheckerObj.AddComponent<GroundChecker>();
            }

            ConfigureGroundChecker(selectable.SelectedInst, groundChecker);
            return groundChecker;
        }

        /// <summary>
        /// selection_creature가 아니라 Creature 본체 SpriteRenderer를 GroundChecker 대상으로 고정합니다.
        /// </summary>
        private void ConfigureGroundChecker(GameObject owner, GroundChecker groundChecker)
        {
            CreatureController creature = owner.GetComponent<CreatureController>();
            if (creature != null && creature.Spriter != null)
            {
                groundChecker.SetTargetRenderer(creature.Spriter);
                return;
            }

            groundChecker.Init();
        }

        /// <summary>
        /// 선택 표시 오브젝트를 가져오거나 GroundChecker 아래에 생성합니다.
        /// </summary>
        private GameObject GetOrCreateSelectionUI(ISelectable selectable, Transform parent)
        {
            if (selectionUIMap.TryGetValue(selectable, out GameObject selectUI) == true && selectUI != null)
            {
                if (selectUI.transform.parent != parent)
                {
                    selectUI.transform.SetParent(parent, false);
                }

                return selectUI;
            }

            selectUI = new GameObject("selection_creature");
            selectUI.transform.SetParent(parent, false);
            selectionUIMap[selectable] = selectUI;

            return selectUI;
        }

        /// <summary>
        /// 선택 표시가 Creature 본체 뒤에 그려지도록 정렬값을 맞춥니다.
        /// </summary>
        private void ApplySelectionSorting(SpriteRenderer targetRenderer, SpriteRenderer selectionRenderer)
        {
            if (selectionRenderer == null)
            {
                return;
            }

            if (targetRenderer == null)
            {
                selectionRenderer.sortingOrder = -1;
                return;
            }

            selectionRenderer.sortingLayerID = targetRenderer.sortingLayerID;
            selectionRenderer.sortingOrder = targetRenderer.sortingOrder - 1;
        }
    }
}
