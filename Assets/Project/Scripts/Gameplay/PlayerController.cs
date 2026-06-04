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

            // 드래깅 중이지 않고
            // 플레이어 호버링하면 외곽선 쉐이딩
            bool isOutlineEnable = !dragger.gameObject.activeSelf && Contains(mouseWorldPos);

            SetOutline(isOutlineEnable);
        }

        private void OnClickPerformed(InputAction.CallbackContext context)
        {
            Vector3 mouseWorldPos = MouseEx.GetMouseWorldPos(Camera.main);
            var indicatorCellPosList = WorldManager.GetMovableCellPosList(Model.CellPos, Model.Directions, Model.IsMoveRepeatable, false);

            // 플레이어 이동중?
            if (HasActionFlag(ActionFlag.Moving)) return;

            // 플레이어를 클릭하면 아군 이동 가능 타일을 표시합니다.
            if (!Contains(mouseWorldPos)) return;

            // 이동할 수 없는 타일 선택?
            if (!WorldManager.TryGetMapCellPos(mouseWorldPos, out _)) return;

            // 드래깅 시작
            // 이동 범위 요청
            // 이동 범위에 있는 크리쳐에게 외곽선 효과 요청
            WorldManager.AddAllyTileIndicator(indicatorCellPosList, this);
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

            // 플레이어 이동중?
            if (HasActionFlag(ActionFlag.Moving)) return;

            // 드래깅 중임?
            if (!dragger.gameObject.activeSelf) return;

            // 공격 가능함?
            if (isAttackable)
            {
                Attack(mouseCellWorldPos, mouseCellPos, monsterController);
            }
            else
            // 이동 가능함?
            if (isMovable)
            {
                // 마우스 커서가 있는 Ground 타일로 이동
                Move(mouseCellWorldPos, mouseCellPos);
            }
            else
            {
                // 원래 타일로 이동
                Move(Model.CellWorldPos, Model.CellPos);
            }

            // 다른 대상 외곽선 쉐이더 종료
            // 드래깅 끝
            // 이동 범위 초기화
            RequestShowOutline(false);
            dragger.gameObject.SetActive(false);
            WorldManager.RemoveTileIndicators(this);

            PlayDrop();
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
