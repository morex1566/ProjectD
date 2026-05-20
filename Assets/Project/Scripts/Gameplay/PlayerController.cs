using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TRPG.Runtime
{
    public partial class PlayerController : CreatureController
    {
        [System.Flags]
        private enum ActionFlag
        {
            None = 0,
            Moving = 1 << 0,
            Attacking = 1 << 1
        }

        private const float MoveInputThreshold = 0.5f;

        private const float AttackMoveRatio = 0.2f;

        private Vector2 moveInput;

        private ActionFlag actionFlags;

        private new PlayerModel Model => base.Model as PlayerModel;




        private void OnEnable()
        {
            InputManager.InputMappingContext.Player.LeftClick.performed += OnClick;
            InputManager.InputMappingContext.Player.Move.performed += OnMove;
            InputManager.InputMappingContext.Player.Move.canceled += OnMoveCanceled;
            moveInput = Vector2.zero;
            actionFlags = ActionFlag.None;
        }

        private void OnDisable()
        {
            InputManager.InputMappingContext.Player.LeftClick.performed -= OnClick;
            InputManager.InputMappingContext.Player.Move.performed -= OnMove;
            InputManager.InputMappingContext.Player.Move.canceled -= OnMoveCanceled;
            moveInput = Vector2.zero;
            actionFlags = ActionFlag.None;
        }

        private void Update()
        {
            UpdateMovement();
        }

        private void UpdateMovement()
        {
            Vector3Int cellStep = GetInputDirection(moveInput);

            Vector3Int targetCellPos = Model.CellPos + cellStep;

            // 이동 안한거임?
            if (cellStep == Vector3Int.zero) return;

            // 이동 또는 공격 액션 중에는 새 입력을 실행하지 않습니다.
            if (HasActionFlag(ActionFlag.Moving | ActionFlag.Attacking)) return;

            // 이동 가능한 타일?
            if (WorldManager.GetInstance().TryGetGroundWorldPosition(targetCellPos, out Vector3 targetWorldPos))
            {
                // 적이 있어서 공격 가능한 타일?
                if (WorldManager.GetInstance().HasMonster(targetCellPos, out MonsterController monsterController))
                {
                    Attack(targetWorldPos, monsterController);
                }
                else
                {
                    Move(targetWorldPos, targetCellPos);
                }

                return;
            }
        }

        /// <summary>
        /// 화면 좌표가 Tilemap 레이어의 유효한 셀이면 해당 셀로 이동합니다.
        /// </summary>
        public void Move(Vector3 targetWorldPos, Vector3Int targetCellPos)
        {
            if (HasActionFlag(ActionFlag.Moving | ActionFlag.Attacking)) return;

            actionFlags |= ActionFlag.Moving;
            StartCoroutine(MovementCo(targetWorldPos, targetCellPos));
        }

        /// <summary>
        /// 적 공격, 적 방향으로 전진했다가 다시 원위치로 이동합니다. 
        /// </summary>
        public void Attack(Vector3 targetWorldPos, CreatureController creatureController)
        {
            if (HasActionFlag(ActionFlag.Moving | ActionFlag.Attacking)) return;

            actionFlags |= ActionFlag.Attacking;
            StartCoroutine(AttackCo(targetWorldPos, creatureController));
        }

        private IEnumerator MovementCo(Vector3 targetWorldPos, Vector3Int targetCellPos)
        {
            Vector3 startWorldPos = transform.position;
            targetWorldPos.z = transform.position.z;

            // 플레이어 전진
            float elapsedTime = 0f;
            while (elapsedTime < Model.Data.MoveDelay)
            {
                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / Model.Data.MoveDelay);
                transform.position = Vector3.Lerp(startWorldPos, targetWorldPos, progress);

                yield return null;
            }

            // 플레이어 전진 끝, 나머지 설정 후처리
            transform.position = targetWorldPos;
            Model.SetCellPos(targetCellPos);
            actionFlags &= ~ActionFlag.Moving;
        }

        private IEnumerator AttackCo(Vector3 targetWorldPos, CreatureController creatureController)
        {
            Vector3 startWorldPos = transform.position;
            targetWorldPos.z = transform.position.z;

            // 공격 이동은 실제 셀을 바꾸지 않고, 대상 방향으로 한 칸 거리의 일부만 왕복합니다.
            Vector3 attackWorldPos = Vector3.Lerp(startWorldPos, targetWorldPos, AttackMoveRatio);
            float halfAttackDelay = Model.Data.AttackDelay * 0.5f;

            // 플레이어 전진
            float elapsedTime = 0f;
            while (elapsedTime < halfAttackDelay)
            {
                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / halfAttackDelay);
                transform.position = Vector3.Lerp(startWorldPos, attackWorldPos, progress);

                yield return null;
            }

            // 공격
            creatureController.Hit(Model.Damage);

            // 플레이어 후진
            elapsedTime = 0f;
            while (elapsedTime < halfAttackDelay)
            {
                elapsedTime += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedTime / halfAttackDelay);
                transform.position = Vector3.Lerp(attackWorldPos, startWorldPos, progress);

                yield return null;
            }

            // 플레이어 공격 끝, 나머지 설정 후처리
            transform.position = startWorldPos;
            actionFlags &= ~ActionFlag.Attacking;
        }

        private bool HasActionFlag(ActionFlag flag)
        {
            return (actionFlags & flag) != ActionFlag.None;
        }

        private Vector3Int GetInputDirection(Vector2 input)
        {
            if (input.sqrMagnitude < MoveInputThreshold * MoveInputThreshold) return Vector3Int.zero;

            // 대각 입력은 허용하지 않고 더 강한 축을 한 칸 이동 방향으로 사용합니다.
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                return input.x > 0f ? Vector3Int.right : Vector3Int.left;
            }

            return input.y > 0f ? Vector3Int.up : Vector3Int.down;
        }
    }

    public partial class PlayerController
    {
        /// <summary>
        /// 클릭 입력으로 화면 좌표 아래의 타일을 검사합니다.
        /// </summary>
        private void OnClick(InputAction.CallbackContext context)
        {
            if (!MouseEx.TryGetMouseWorldPosition(Camera.main, out Vector3 screenWorldPos)) return;

            // TODO : 전투씬에서만 사용하도록 return;

            // TODO : ObjectSelector 대상으로만 한정?

            // 클릭한 위치가 이동할 수 있는 곳임?
            if (WorldManager.GetInstance().TryGetGroundCellPosition(screenWorldPos, out Vector3Int cellPos))
            {

            }

            // 공격할 수 있는곳임?
            if (WorldManager.GetInstance().HasMonster(cellPos, out MonsterController monster))
            {

            }
        }

        private void OnMove(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            moveInput = Vector2.zero;
        }
    }
}
