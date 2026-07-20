using System.Collections.Generic;
using UnityEngine;

namespace TRPG.Runtime
{
    /// <summary>
    /// Creature 클릭 선택과 드래그 선택 로직을 처리합니다.
    /// </summary>
    public sealed class CreatureSelector : Selector<CreatureController>
    {
        [SerializeField] private Sprite selectedCreatureSprite = null;


        private readonly HashSet<CreatureController> selectedCreatures = new();

        private readonly Dictionary<CreatureController, GameObject> selectionUIMap = new();

        /// <summary>
        /// 드래그 사각형 안에 들어온 크리처의 선택 표시를 갱신합니다.
        /// </summary>
        protected override void SelectTargets(Camera cam, Vector2 startScreenPosition, Vector2 endScreenPosition)
        {
            SetSelection(FindCreatures(cam, startScreenPosition, endScreenPosition));
        }

        /// <summary>
        /// 클릭 위치를 포함하는 크리처 하나의 선택 표시를 갱신합니다.
        /// </summary>
        protected override void SelectTarget(Camera cam, Vector2 pointerWorldPosition)
        {
            HashSet<CreatureController> currentSelectedCreatures = new();

            CreatureController creature = FindBestCreature(pointerWorldPosition);
            if (creature != null)
            {
                currentSelectedCreatures.Add(creature);
            }

            SetSelection(currentSelectedCreatures);
        }

        /// <summary>
        /// 현재 선택 표시가 남아 있는 크리처를 확정 Target으로 등록합니다.
        /// </summary>
        protected override void CompleteSelection()
        {
            targets.Clear();

            foreach (CreatureController creature in selectedCreatures)
            {
                AddTarget(creature);
            }
        }

        /// <summary>
        /// 현재 확정 선택 목록과 표시를 제거합니다.
        /// </summary>
        protected override void ClearTarget()
        {
            foreach (GameObject selectionUI in selectionUIMap.Values)
            {
                if (selectionUI != null)
                {
                    Destroy(selectionUI);
                }
            }

            selectionUIMap.Clear();
            selectedCreatures.Clear();
            targets.Clear();
        }

        /// <summary>
        /// 확정 Target 목록에 크리처를 중복 없이 추가합니다.
        /// </summary>
        protected override void AddTarget(CreatureController creature)
        {
            if (creature == null || targets.Contains(creature) == true)
            {
                return;
            }

            targets.Add(creature);
        }

        /// <summary>
        /// 현재 드래그 또는 클릭 범위와 선택 표시 상태를 동기화합니다.
        /// </summary>
        private void SetSelection(IEnumerable<CreatureController> currentSelectedCreatures)
        {
            HashSet<CreatureController> currentSelection = new(currentSelectedCreatures);
            List<CreatureController> removedCreatures = new();

            foreach (CreatureController creature in selectedCreatures)
            {
                if (currentSelection.Contains(creature) == false)
                {
                    removedCreatures.Add(creature);
                }
            }

            foreach (CreatureController creature in removedCreatures)
            {
                selectedCreatures.Remove(creature);
                RemoveSelectionUI(creature);
            }

            foreach (CreatureController creature in currentSelection)
            {
                if (creature == null || selectedCreatures.Add(creature) == false)
                {
                    continue;
                }

                if (TrySetSelectionUI(creature) == false)
                {
                    selectedCreatures.Remove(creature);
                }
            }
        }

        /// <summary>
        /// 드래그 화면 영역과 겹치는 등록된 크리처를 반환합니다.
        /// </summary>
        private static List<CreatureController> FindCreatures(Camera cam, Vector2 startScreenPosition, Vector2 endScreenPosition)
        {
            List<CreatureController> creatures = new();
            Rect selectionRect = ScreenEx.CreateScreenRect(startScreenPosition, endScreenPosition);

            foreach (CreatureController creature in WorldManager.Creatures.Values)
            {
                if (creature == null || creature.isActiveAndEnabled == false || creature.Spriter == null)
                {
                    continue;
                }

                Bounds selectionBounds = creature.HitBox != null ? creature.HitBox.bounds : creature.Spriter.bounds;
                if (ScreenEx.IsWorldObjBoundsInScreenRect(cam, selectionBounds, selectionRect) == true)
                {
                    creatures.Add(creature);
                }
            }

            return creatures;
        }

        /// <summary>
        /// 클릭 위치를 포함하는 크리처 중 중심이 가장 가까운 대상을 반환합니다.
        /// </summary>
        private static CreatureController FindBestCreature(Vector2 pointerWorldPosition)
        {
            CreatureController bestCreature = null;
            float bestSqrDistance = float.MaxValue;
            int bestInstanceId = int.MaxValue;

            foreach (CreatureController creature in WorldManager.Creatures.Values)
            {
                if (creature == null || creature.isActiveAndEnabled == false || creature.Spriter == null)
                {
                    continue;
                }

                Bounds bounds = creature.Spriter.bounds;
                if (bounds.Contains(pointerWorldPosition) == false)
                {
                    continue;
                }

                float sqrDistance = ((Vector2)bounds.center - pointerWorldPosition).sqrMagnitude;
                int instanceId = creature.InstanceId;

                if (sqrDistance > bestSqrDistance)
                {
                    continue;
                }

                if (Mathf.Approximately(sqrDistance, bestSqrDistance) == true && instanceId >= bestInstanceId)
                {
                    continue;
                }

                bestCreature = creature;
                bestSqrDistance = sqrDistance;
                bestInstanceId = instanceId;
            }

            return bestCreature;
        }

        /// <summary>
        /// 크리처 발바닥 위치에 선택 표시를 생성하거나 갱신합니다.
        /// </summary>
        private bool TrySetSelectionUI(CreatureController creature)
        {
            if (creature == null || creature.Spriter == null || selectedCreatureSprite == null)
            {
                return false;
            }

            GroundChecker groundChecker = GetOrCreateGroundChecker(creature);
            groundChecker.SetTargetRenderer(creature.Spriter);
            groundChecker.Generate();

            GameObject selectionUI = GetOrCreateSelectionUI(creature, groundChecker.transform);
            selectionUI.transform.localPosition = Vector3.zero;
            selectionUI.transform.localRotation = Quaternion.identity;
            selectionUI.transform.localScale = Vector3.one;

            SpriteRenderer selectionRenderer = selectionUI.GetComponent<SpriteRenderer>();
            if (selectionRenderer == null)
            {
                selectionRenderer = selectionUI.AddComponent<SpriteRenderer>();
            }

            selectionRenderer.sprite = selectedCreatureSprite;
            selectionRenderer.sortingLayerID = creature.Spriter.sortingLayerID;
            selectionRenderer.sortingOrder = creature.Spriter.sortingOrder - 1;

            return true;
        }

        /// <summary>
        /// 크리처에 연결된 GroundChecker를 가져오거나 생성합니다.
        /// </summary>
        private static GroundChecker GetOrCreateGroundChecker(CreatureController creature)
        {
            GroundChecker groundChecker = creature.GetComponentInChildren<GroundChecker>();

            if (groundChecker != null)
            {
                return groundChecker;
            }

            GameObject groundCheckerObject = new GameObject(nameof(GroundChecker));
            groundCheckerObject.transform.SetParent(creature.transform, false);

            return groundCheckerObject.AddComponent<GroundChecker>();
        }

        /// <summary>
        /// 크리처 선택 표시 오브젝트를 가져오거나 생성합니다.
        /// </summary>
        private GameObject GetOrCreateSelectionUI(CreatureController creature, Transform parent)
        {
            if (selectionUIMap.TryGetValue(creature, out GameObject selectionUI) == true && selectionUI != null)
            {
                if (selectionUI.transform.parent != parent)
                {
                    selectionUI.transform.SetParent(parent, false);
                }

                return selectionUI;
            }

            selectionUI = new GameObject("selection_creature");
            selectionUI.transform.SetParent(parent, false);
            selectionUIMap[creature] = selectionUI;

            return selectionUI;
        }

        /// <summary>
        /// 지정한 크리처의 선택 표시를 제거합니다.
        /// </summary>
        private void RemoveSelectionUI(CreatureController creature)
        {
            if (selectionUIMap.TryGetValue(creature, out GameObject selectionUI) == false)
            {
                return;
            }

            selectionUIMap.Remove(creature);

            if (selectionUI != null)
            {
                Destroy(selectionUI);
            }
        }
    }
}
