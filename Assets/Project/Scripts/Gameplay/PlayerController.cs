using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements.Experimental;

namespace TRPG.Runtime
{
    /// <summary>
    /// 플레이어 크리처의 입력 처리와 이동 행동을 담당합니다.
    /// </summary>
    public partial class PlayerController : CreatureController
    {
        [SerializeField] private ObjectSelector2D selector;

        public new PlayerModel Model => base.Model as PlayerModel;

        protected override void Awake()
        {
            base.Awake();
        }

        private void OnEnable()
        {
            InputManager.InputMappingContext.Player.LeftClick.performed += OnClickPerformed;
            InputManager.InputMappingContext.Player.LeftClick.canceled += OnClickCanceled;
            InputManager.InputMappingContext.Player.Point.performed += OnPointPerformed;

            PlaySpawnAnim();
        }

        private void OnDisable()
        {
            InputManager.InputMappingContext.Player.LeftClick.performed -= OnClickPerformed;
            InputManager.InputMappingContext.Player.LeftClick.canceled -= OnClickCanceled;
            InputManager.InputMappingContext.Player.Point.performed -= OnPointPerformed;
        }


        public void SetActiveSelector(bool active)
        {
            selector.gameObject.SetActive(active);
        }

        /// <summary>
        /// 이동 범위에 있는 크리쳐에게 외곽선 효과 요청
        /// </summary>
        private void RequestShowOutline(bool active)
        {
            var movableCellPosList = WorldManager.GetMovableCellPosList(Model.CellPos, Model.Directions, Model.IsMoveRepeatable, true);

            List<CreatureController> creatureControllers = WorldManager.GetCreaturesInCellPosList(movableCellPosList);
            foreach (CreatureController creatureController in creatureControllers)
            {
                creatureController.SetOutline(active);
            }
        }

        /// <summary>
        /// 마우스 호버링이 적에 있는지?
        /// 이동 가능한 범위에 적이 있는지?
        /// </summary>
        private bool IsAttackable(Vector3 mouseWorldPos, out MonsterController monsterController)
        {
            var movableCellPosList = WorldManager.GetMovableCellPosList(Model.CellPos, Model.Directions, Model.IsMoveRepeatable, true);
            WorldManager.TryGetMapCellPos(mouseWorldPos, out Vector3Int mouseCellPos);

            bool isAttackable = WorldManager.HasMonsterInCellPos(mouseCellPos, out MonsterController outMonsterController) &&
                                movableCellPosList.Contains(mouseCellPos);

            monsterController = outMonsterController;

            return isAttackable;
        }

        /// <summary>
        /// 이동 가능한 타일인지?
        /// 이동 가능한 범위인지?
        /// </summary>
        private bool IsMovable(Vector3 mouseWorldPos, out Vector3 mouseCellWorldPos, out Vector3Int mouseCellPos)
        {
            var movableCellPosList = WorldManager.GetMovableCellPosList(Model.CellPos, Model.Directions, Model.IsMoveRepeatable, true);

            bool isMovable = WorldManager.TryGetMapCellPos(mouseWorldPos, out Vector3Int outMouseCellPos) &&
                             movableCellPosList.Contains(outMouseCellPos);

            WorldManager.TryGetMapWorldPos(outMouseCellPos, out Vector3 outMouseCellWorldPos);

            mouseCellPos = outMouseCellPos;
            mouseCellWorldPos = outMouseCellWorldPos;

            return isMovable;
        }

        /// <summary>
        /// 체스 말처럼 적이 점유한 이동 가능 칸을 공격 가능 칸으로 분리합니다.
        /// </summary>
        private List<Vector3Int> GetAttackableCellPosList()
        {
            var movableCellPosList = WorldManager.GetMovableCellPosList(Model.CellPos, Model.Directions, Model.IsMoveRepeatable, true);
            List<Vector3Int> attackableCellPosList = new();

            foreach (Vector3Int cellPos in movableCellPosList)
            {
                if (!WorldManager.HasMonsterInCellPos(cellPos, out _)) continue;

                attackableCellPosList.Add(cellPos);
            }

            return attackableCellPosList;
        }

        /// <summary>
        /// 드래그 중 마우스가 가리키는 이동/공격 인디케이터를 hover 상태로 갱신합니다.
        /// </summary>
        private void RequestTileIndicatorHover(Vector3 mouseWorldPos)
        {
            if (WorldManager.TryGetMapCellPos(mouseWorldPos, out Vector3Int mouseCellPos) &&
                WorldManager.HasIndicatorInCellPos(mouseCellPos, this))
            {
                WorldManager.SetTileIndicatorHover(this, mouseCellPos);
                return;
            }

            WorldManager.ClearTileIndicatorHover(this);
        }
    }

    /// <summary>
    /// 입력
    /// </summary>
    public partial class PlayerController
    {
        private void OnPointPerformed(InputAction.CallbackContext context)
        {
            Vector3 mouseWorldPos = MouseEx.GetMouseWorldPos(Camera.main);

            // 플레이어 이동중?
            if (HasActionFlag(ActionFlag.Moving)) return;

            if (!WorldManager.CanPlayerAct) return;

            if (dragger.gameObject.activeSelf)
            {
                RequestTileIndicatorHover(mouseWorldPos);
                return;
            }

            // 드래깅 중이지 않고
            // 플레이어 호버링하면 외곽선 쉐이딩
            bool isOutlineEnable = Contains(mouseWorldPos);

            // 현재 대상에 대상 외곽선 쉐이딩
            SetOutline(isOutlineEnable);
        }

        private void OnClickPerformed(InputAction.CallbackContext context)
        {
            Vector3 mouseWorldPos = MouseEx.GetMouseWorldPos(Camera.main);
            var indicatorCellPosList = WorldManager.GetMovableCellPosList(Model.CellPos, Model.Directions, Model.IsMoveRepeatable, false);
            var attackableCellPosList = GetAttackableCellPosList();

            // 플레이어 이동중?
            if (HasActionFlag(ActionFlag.Moving)) return;

            if (!WorldManager.CanPlayerAct) return;

            // 플레이어를 클릭하면 아군 이동 가능 타일을 표시합니다.
            if (!Contains(mouseWorldPos)) return;

            // 이동할 수 없는 타일 선택?
            if (!WorldManager.TryGetMapCellPos(mouseWorldPos, out _)) return;

            // 드래깅 시작
            // 이동 범위 요청
            // 이동 범위에 있는 크리쳐에게 외곽선 효과 요청
            WorldManager.AddTileIndicators(indicatorCellPosList, attackableCellPosList, this);
            dragger.gameObject.SetActive(true);
            SetOutline(false);
            RequestShowOutline(true);

            PlayPick();
        }

        private void OnClickCanceled(InputAction.CallbackContext context)
        {
            Vector3 mouseWorldPos = MouseEx.GetMouseWorldPos(Camera.main);
            bool isAttackable = IsAttackable(mouseWorldPos, out MonsterController monsterController);
            bool isMovable = IsMovable(mouseWorldPos, out Vector3 mouseCellWorldPos, out Vector3Int mouseCellPos);
            bool isTurnAction = isAttackable || isMovable;

            // 플레이어 이동중?
            if (HasActionFlag(ActionFlag.Moving)) return;

            if (!WorldManager.CanPlayerAct) return;

            // 드래깅 중임?
            if (!dragger.gameObject.activeSelf) return;

            // 공격 가능함? -> 해당 타일로 이동 + 공격
            if (isAttackable)
            {
                Attack(mouseCellWorldPos, mouseCellPos, monsterController);
            }
            else
            // 이동 가능함? -> 해당 타일로 이동
            if (isMovable)
            {
                Move(mouseCellWorldPos, mouseCellPos);
            }
            // 공격도, 이동도 못함? -> 원래 타일로
            else
            {
                Move(Model.CellWorldPos, Model.CellPos);
            }

            // 다른 대상 외곽선 쉐이더 종료
            // 드래깅 끝
            // 이동 범위 초기화
            RequestShowOutline(false);
            dragger.gameObject.SetActive(false);
            WorldManager.RemoveTileIndicators(this);

            PlayDrop();

            if (isTurnAction)
            {
                StartCoroutine(EndPlayerTurnWhenActionCompletedCoroutine());
            }
        }

        private IEnumerator EndPlayerTurnWhenActionCompletedCoroutine()
        {
            yield return null;

            while (IsActing)
            {
                yield return null;
            }

            WorldManager.EndPlayerTurn();
        }
    }

    /// <summary>
    /// 애니메이션
    /// </summary>
    public partial class PlayerController
    {
        private void PlaySpawnAnim()
        {
            // 플레이어 스폰 상태로 전환합니다.
            animator.SetTrigger(UnityConstant.Animator.Parameters.AC_Gameplay_Creature.Trigger.OnSpawn);
        }
    }
}
